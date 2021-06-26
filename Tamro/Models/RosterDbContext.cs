using System;
using Microsoft.EntityFrameworkCore;

namespace Tamro.Models
{
    public class RosterDbContext : DbContext
    {
        public RosterDbContext(DbContextOptions<RosterDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(s => s.Id);
                user.Property(s => s.Name).IsRequired();
                user.Property(s => s.Surname).IsRequired();
            });
        }
    }
}
