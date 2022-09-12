using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoreographyExample.DAL
{
    public class VotingDbContext : DbContext
    {
        public VotingDbContext(DbContextOptions<VotingDbContext> options)
        : base(options)
        {
        }
        public DbSet<Voting> Votings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Voting>()
                .HasMany(c => c.Votes)
                .WithOne(e => e.Voting);

            base.OnModelCreating(modelBuilder);
        }
    }
}
