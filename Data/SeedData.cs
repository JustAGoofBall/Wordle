using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Worlde.Models;

namespace Worlde.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new WorldeContext(
                serviceProvider.GetRequiredService<DbContextOptions<WorldeContext>>());

            if (context.Word.Any())
                return;

            // Pad naar het tekstbestand met woorden
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "words.txt");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("words.txt niet gevonden.", filePath);

            var words = File.ReadAllLines(filePath)
                            .Select(w => w.Trim().ToLower())
                            .Where(w => w.Length == 5 && w.All(char.IsLetter))
                            .Distinct();

            foreach (var w in words)
            {
                context.Word.Add(new Word
                {
                    Letters = w,
                    Length = w.Length,
                    TimesGuessed = 0,
                    Created = DateTime.UtcNow
                });
            }

            context.SaveChanges();
        }
    }
}