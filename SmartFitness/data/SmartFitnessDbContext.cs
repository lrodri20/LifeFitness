
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Data
{
    public class SmartFitnessDbContext : DbContext
    {
        public SmartFitnessDbContext(DbContextOptions<SmartFitnessDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        // Add other DbSets for your entities
    }
}