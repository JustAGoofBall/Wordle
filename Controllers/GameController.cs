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

            // Maximaal 6 pogingen
            if (gameData.Guesses.Count >= 6)
            {
                ViewBag.ColoredGuesses = gameData.Guesses;
                ViewBag.CurrentWord = word;
                ViewBag.Error = "Je hebt het maximale aantal pogingen bereikt.";
                return View();
            }

            // Alleen verwerken als het woord 5 letters is en bestaat in de database
            if (!string.IsNullOrWhiteSpace(guess) && guess.Length == 5 && !string.IsNullOrEmpty(word))
            {
                guess = guess.ToLower();

                // Controleer of het woord bestaat in de database
                bool wordExists = _context.Word.Any(w => w.Letters == guess);
                if (!wordExists)
                {
                    ViewBag.ColoredGuesses = gameData.Guesses;
                    ViewBag.CurrentWord = word;
                    ViewBag.Error = "Dit woord bestaat niet in de woordenlijst.";
                    return View();
                }

                word = word.ToLower();

                var coloredGuess = new ColoredGuess
                {
                    Guess = guess,
                    Letters = new List<LetterResult>()
                };

                var wordChars = word.ToCharArray();
                var guessChars = guess.ToCharArray();
                var letterColors = new string[5];
                var wordCharUsed = new bool[5];

                // 1. Groen toewijzen (juiste letter op juiste plek)
                for (int i = 0; i < 5; i++)
                {
                    if (guessChars[i] == wordChars[i])
                    {
                        letterColors[i] = "green";
                        wordCharUsed[i] = true;
                    }
                }

                // 2. Geel/grijs toewijzen
                for (int i = 0; i < 5; i++)
                {
                    if (letterColors[i] == "green")
                    {
                        coloredGuess.Letters.Add(new LetterResult { Letter = guessChars[i], Color = "green" });
                    }
                    else
                    {
                        // Kijk of deze letter nog ergens anders in het woord voorkomt (en nog niet gebruikt is)
                        bool found = false;
                        for (int j = 0; j < 5; j++)
                        {
                            if (!wordCharUsed[j] && guessChars[i] == wordChars[j])
                            {
                                found = true;
                                wordCharUsed[j] = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            letterColors[i] = "yellow";
                            coloredGuess.Letters.Add(new LetterResult { Letter = guessChars[i], Color = "yellow" });
                        }
                        else
                        {
                            letterColors[i] = "gray";
                            coloredGuess.Letters.Add(new LetterResult { Letter = guessChars[i], Color = "gray" });
                        }
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