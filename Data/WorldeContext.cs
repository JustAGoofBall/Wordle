using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Worlde.Models;

namespace Worlde.Data
{
    public class WorldeContext : DbContext
    {
        public WorldeContext (DbContextOptions<WorldeContext> options)
            : base(options)
        {
        }

        public DbSet<Worlde.Models.Word> Word { get; set; } = default!;
    }
}
