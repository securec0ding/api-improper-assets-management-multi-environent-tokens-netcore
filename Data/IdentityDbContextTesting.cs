using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Data
{
    public class IdentityDbContextTesting : IdentityDbContext<IdentityUserTesting, IdentityRoleTesting, string>
    {
        public IdentityDbContextTesting(DbContextOptions options) : base(options)
        {

        }
    }
}