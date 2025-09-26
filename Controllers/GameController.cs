using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Worlde.Data;
using Worlde.Models;
using System.Text.Json;

namespace Worlde.Controllers
{
    public class GameController : Controller
    {
        private readonly WorldeContext _context;

        public GameController(WorldeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Session ophalen
            var session = HttpContext.Session;

            // Woord ophalen of kiezen
            string? word = session.GetString("CurrentWord");
            if (string.IsNullOrEmpty(word))
            {
                word = await _context.Word
                    .OrderBy(r => Guid.NewGuid())
                    .Select(w => w.Letters)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(word))
                    session.SetString("CurrentWord", word);
            }

            // GameData ophalen
            var gameDataJson = session.GetString("GameData");
            GameData gameData = string.IsNullOrEmpty(gameDataJson)
                ? new GameData()
                : JsonSerializer.Deserialize<GameData>(gameDataJson) ?? new GameData();

            ViewBag.Guesses = gameData.Guesses;
            ViewBag.CurrentWord = word;
            ViewBag.ColoredGuesses = gameData.Guesses;
            return View();
        }

        [HttpPost]
        public IActionResult Index(string guess)
        {
            var session = HttpContext.Session;

            // GameData ophalen
            var gameDataJson = session.GetString("GameData");
            GameData gameData = string.IsNullOrEmpty(gameDataJson)
                ? new GameData()
                : JsonSerializer.Deserialize<GameData>(gameDataJson) ?? new GameData();

            string? word = session.GetString("CurrentWord");

            if (!string.IsNullOrWhiteSpace(guess) && guess.Length == 5 && !string.IsNullOrEmpty(word))
            {
                guess = guess.ToLower();
                word = word.ToLower();

                var coloredGuess = new ColoredGuess
                {
                    Guess = guess,
                    Letters = new List<LetterResult>()
                };

                var wordChars = word.ToCharArray();
                var guessChars = guess.ToCharArray();
                var letterColors = new string[5];

                // Eerst groen toewijzen
                for (int i = 0; i < 5; i++)
                {
                    if (guessChars[i] == wordChars[i])
                    {
                        letterColors[i] = "green";
                        wordChars[i] = '*'; // Markeer als gebruikt
                    }
                }

                // Daarna geel/grijs toewijzen
                for (int i = 0; i < 5; i++)
                {
                    if (letterColors[i] == "green")
                    {
                        coloredGuess.Letters.Add(new LetterResult { Letter = guessChars[i], Color = "green" });
                    }
                    else if (wordChars.Contains(guessChars[i]))
                    {
                        letterColors[i] = "yellow";
                        // Markeer eerste gevonden letter als gebruikt
                        for (int j = 0; j < 5; j++)
                        {
                            if (wordChars[j] == guessChars[i])
                            {
                                wordChars[j] = '*';
                                break;
                            }
                        }
                        coloredGuess.Letters.Add(new LetterResult { Letter = guessChars[i], Color = "yellow" });
                    }
                    else
                    {
                        letterColors[i] = "gray";
                        coloredGuess.Letters.Add(new LetterResult { Letter = guessChars[i], Color = "gray" });
                    }
                }

                gameData.Guesses.Add(coloredGuess);
                session.SetString("GameData", JsonSerializer.Serialize(gameData));
            }

            ViewBag.ColoredGuesses = gameData.Guesses;
            ViewBag.CurrentWord = word;
            return View();
        }

        public IActionResult Reset()
        {
            HttpContext.Session.Remove("GameData");
            HttpContext.Session.Remove("CurrentWord");
            return RedirectToAction("Index");
        }
    }
}