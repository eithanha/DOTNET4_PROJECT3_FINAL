using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using PlotPocket.Server.Models;

namespace PlotPocket.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] EmailLoginDetails details)
        {
            // Create User instance
            var user = new User
            {
                UserName = details.Email,
                Email = details.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create the user
            var result = await _userManager.CreateAsync(user, details.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.First().Description);
            }

            // Don't send sensitive data back to the client
            user.PasswordHash = null;
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<ActionResult> Test()
        {
            /**
                Simple endpoint that can be used to see if your 
                authenitcation system is working.
            */
            return Ok(new { message = "hello" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] EmailLoginDetails details)
        {
            // Retrieve user
            var user = await _userManager.FindByEmailAsync(details.Email);

            if (user == null)
            {
                return BadRequest("Invalid email or password");
            }

            // Check if the password is correct & attempt to sign in
            var result = await _signInManager.PasswordSignInAsync(user, details.Password, false, false);

            if (!result.Succeeded)
            {
                return BadRequest("Invalid email or password");
            }

            // Don't send sensitive data back to the client
            user.PasswordHash = null;
            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}