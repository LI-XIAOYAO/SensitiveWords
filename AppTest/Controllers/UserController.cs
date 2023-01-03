using Microsoft.AspNetCore.Mvc;

namespace AppTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetUser")]
        public User Get(string name)
        {
            return new User
            {
                Name = name,
                Mail = "11223344@mail.com",
                Phone = "16655553333"
            };
        }
    }
}