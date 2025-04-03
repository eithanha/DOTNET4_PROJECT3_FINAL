using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

            var user = new User
            {
                UserName = details.Email,
                Email = details.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };


            var result = await _userManager.CreateAsync(user, details.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.First().Description);
            }


            await _signInManager.SignInAsync(user, isPersistent: false);


            user.PasswordHash = null;
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<ActionResult> Test()
        {
            try
            {

                var userCount = await _userManager.Users.CountAsync();
                return Ok(new { message = "Connection test successful", userCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Connection test failed", message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] EmailLoginDetails details)
        {

            var user = await _userManager.FindByEmailAsync(details.Email);

            if (user == null)
            {
                return BadRequest("Invalid email or password");
            }


            var result = await _signInManager.PasswordSignInAsync(user, details.Password, false, false);

            if (!result.Succeeded)
            {
                return BadRequest("Invalid email or password");
            }


            user.PasswordHash = null;
            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("status")]
        public async Task<ActionResult<User>> GetAuthStatus()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }


            user.PasswordHash = null;
            return Ok(user);
        }
    }
}