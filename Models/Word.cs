using System;
using System.ComponentModel.DataAnnotations;

namespace Worlde.Models
{
    public class Word
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Letters { get; set; } = string.Empty;

        public int Length { get; set; }

        public DateTime Created { get; set; }
    }
}
