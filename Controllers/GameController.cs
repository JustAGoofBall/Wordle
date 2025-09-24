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

            if (!string.IsNullOrWhiteSpace(guess) && guess.Length == 5)
            {
                gameData.Guesses.Add(guess.ToLower());
                session.SetString("GameData", JsonSerializer.Serialize(gameData));
            }

            ViewBag.Guesses = gameData.Guesses;
            ViewBag.CurrentWord = session.GetString("CurrentWord");
            return View();
        }
    }
}