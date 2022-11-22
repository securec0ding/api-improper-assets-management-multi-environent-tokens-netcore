using Backend.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class ApplicationDbContextTesting : IdentityDbContext<IdentityUserTesting, IdentityRoleTesting, string>
    {
        public DbSet<BankAccount> Accounts { get; set; }

        public ApplicationDbContextTesting(DbContextOptions<ApplicationDbContextTesting> options)
            : base(options)
        {
        }
    }
}