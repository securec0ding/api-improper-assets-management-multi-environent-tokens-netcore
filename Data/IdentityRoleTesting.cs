using Microsoft.AspNetCore.Identity;

namespace Backend.Data
{
    public class IdentityRoleTesting : IdentityRole
    {
        public IdentityRoleTesting() : base()
        {

        }

        public IdentityRoleTesting(string roleName) : base(roleName)
        {

        }
    }
}