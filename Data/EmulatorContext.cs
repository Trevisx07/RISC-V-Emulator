using Microsoft.EntityFrameworkCore;
using RiscVEmulator.Models;

namespace RiscVEmulator.Data
{
    public class EmulatorContext : DbContext
    {
        public DbSet<ProgramRecord> Programs { get; set; }

        public EmulatorContext(DbContextOptions<EmulatorContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=emulator.db");
            }
        }
    }
}