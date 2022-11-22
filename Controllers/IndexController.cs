using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class IndexController : ControllerBase
    {
        [HttpGet]
        [Route("/")]
        public string Get()
        {
            return "API is running";
        }
    }
}