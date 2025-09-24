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

            var words = new[]
            {
                "apple", "grape", "pearl", "table", "chair",
                "plant", "bread", "flame", "crane", "sword"
            };

            foreach (var w in words)
            {
                context.Word.Add(new Word
                {
                    Letters = w,
                    Length = w.Length,
                    Created = DateTime.UtcNow
                });
            }

            context.SaveChanges();
        }
    }
}