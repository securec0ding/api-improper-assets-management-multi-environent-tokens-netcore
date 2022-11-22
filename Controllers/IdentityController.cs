using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/api/v2")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService service;

        public IdentityController(IIdentityService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Auth([FromBody] LoginModel credentials)
        {
            var isPasswordCorrect = await this.service.IsPasswordCorrectAsync(credentials.UserName, credentials.Password);
            if (!isPasswordCorrect)
                return StatusCode(401, new { Message = "Incorrect username or password" });

            var token = await this.service.IssueJwtTokenAsync(credentials.UserName);

            var result = new AuthenticationTokenModel { Token = token };
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        [Authorize]
        public async Task<IActionResult> Info()
        {
            var userInfo = await this.service.GetUserAsync(User.Identity.Name);
            return Ok(userInfo);
        }
    }
}