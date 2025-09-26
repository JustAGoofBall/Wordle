using System.Collections.Generic;

namespace Worlde.Models
{
    public class GameData
    {
        public List<ColoredGuess> Guesses { get; set; } = new();
        public bool IsGameOver { get; set; } = false;
        public bool IsWin { get; set; } = false;
    }
}