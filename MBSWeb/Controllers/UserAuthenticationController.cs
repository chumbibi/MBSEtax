using MBSWeb.Managers;
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
 

namespace MBSWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthenticationController : ControllerBase
    {
        private readonly UserAuthenticationManager _manager;

        public UserAuthenticationController(UserAuthenticationManager manager)
        {
            _manager = manager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto model)
        {
            var result = await _manager.AuthenticateUserAsync(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var result = await _manager.RegisterUserAsync(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var result = await _manager.ChangePasswordAsync(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email)
        {
            var result = await _manager.ResetPasswordAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("unregister")]
        public async Task<IActionResult> Unregister([FromQuery] string email)
        {
            var result = await _manager.UnregisterUserAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _manager.GetAllUsersAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("usersbyemail")]
        public async Task<IActionResult> GetUsersByEmail([FromQuery] string email)
        {
            var result = await _manager.GetUsersByEmailAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var result = await _manager.GetUserRolesAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("rolesbyuser")]
        public async Task<IActionResult> GetRoleByUser([FromQuery] string email)
        {
            var result = await _manager.GetRoleByUserAsync(email);
            return StatusCode(result.StatusCode, result);
        }
    }
}
