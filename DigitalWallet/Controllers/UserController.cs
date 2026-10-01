using DigitalWallet.Common.DTO;
using DigitalWallet.Service;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWallet.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto userDto)
        {
            var result = await _userService.LoginAsync(userDto);
            if (result is null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }

        [HttpPost()]
        public async Task<IActionResult> AddUser(UserRegitration user)
        {
            var created = await _userService.CreateAsync(user);
            if (created is null)
            {
                return Conflict("A user with this email already exists.");
            }

            return Ok(created);
        }
    }
}
