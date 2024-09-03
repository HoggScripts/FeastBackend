using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Feast.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;

  
        public ICollection<Recipe> Recipes { get; set; }
        

        public GoogleOAuthToken GoogleOAuthToken { get; set; }

        // Meal time settings with default values
        public TimeSpan BreakfastTime { get; set; } = TimeSpan.FromHours(8); // Default to 8
        public TimeSpan LunchTime { get; set; } = TimeSpan.FromHours(12);    // Default to 12
        public TimeSpan DinnerTime { get; set; } = TimeSpan.FromHours(18);   // Default to 6
    }
}