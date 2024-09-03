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
        public DbSet<GoogleOAuthToken> GoogleOAuthTokens { get; set; } 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the recipe
            builder.Entity<Recipe>()
                .OwnsMany(r => r.Ingredients, a =>
                {
                    a.WithOwner().HasForeignKey("RecipeId");
                    a.Property<int>("Id"); //primary key
                    a.HasKey("RecipeId", "Id"); // Composite key
                });

            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MealType to be stored as a string
            builder.Entity<Recipe>()
                .Property(r => r.MealType)
                .HasConversion(
                    v => v,  
                    v => v); 

   
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.GoogleOAuthToken)
                .WithOne(t => t.User)
                .HasForeignKey<GoogleOAuthToken>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}