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
    }
}