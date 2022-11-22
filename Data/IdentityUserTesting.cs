using Microsoft.AspNetCore.Identity;

namespace Backend.Data
{
    public class IdentityUserTesting : IdentityUser
    {
        public IdentityUserTesting() : base()
        {

        }

        public IdentityUserTesting(string userName) : base (userName)
        {

        }
    }
}