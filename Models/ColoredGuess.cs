using System.Collections.Generic;

namespace Worlde.Models
{
    public class ColoredGuess
    {
        public string Guess { get; set; } = string.Empty;
        public List<LetterResult> Letters { get; set; } = new();
    }

    public class LetterResult
    {
        public char Letter { get; set; }
        public string Color { get; set; } = "gray"; // "green", "yellow", "gray"
    }
}
