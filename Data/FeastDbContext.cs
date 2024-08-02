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
                .OwnsMany(r => r.Ingredients, a =>
                {
                    a.WithOwner().HasForeignKey("RecipeId");
                    a.Property<int>("Id"); // Keep the Id as a primary key
                    a.HasKey("RecipeId", "Id"); // Composite key
                });

            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}