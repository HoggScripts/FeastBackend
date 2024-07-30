using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Feast.Models;

namespace Feast.Data
{
    public class FeastDbContext : IdentityDbContext<ApplicationUser>
    {
        public FeastDbContext(DbContextOptions<FeastDbContext> options) : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)  // Ensure the user has a collection of recipes
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Specify delete behavior
        }
    }
}