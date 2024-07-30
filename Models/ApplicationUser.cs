using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Feast.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;

        // Add a collection of recipes
        public ICollection<Recipe> Recipes { get; set; }
    }
}