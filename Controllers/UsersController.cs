using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API.Modelo;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _userService.PasswordSignInAsync(model.Username, model.Password);
            if (result.Succeeded)
            {
                return Ok(new { message = "Login successful" });
            }
            return Unauthorized(new { message = "Invalid login attempt" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userService.GetAuthenticatedUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Address,
                user.FarmName
            });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
        {
            var user = await _userService.GetAuthenticatedUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var result = await _userService.UpdateUserAsync(user, model);
            if (result.Succeeded)
            {
                return Ok(new { message = "User updated successfully" });
            }
            return BadRequest(new { message = "User update failed", errors = result.Errors });
        }

        // Este método es un ejemplo de cómo manejar rutas que contienen un userId
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            // Verificamos si el usuario autenticado tiene permiso para acceder a esta información
            if (!await _userService.IsUserAuthenticated(User))
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            // Aquí podrías agregar lógica adicional para verificar si el usuario autenticado
            // tiene permiso para acceder a la información de userId específico

            var user = await _userService.GetAuthenticatedUserAsync(User);
            if (user == null || user.Id != userId)
            {
                return Forbid(new { message = "Access denied" });
            }

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Address,
                user.FarmName
            });
        }
    }
}