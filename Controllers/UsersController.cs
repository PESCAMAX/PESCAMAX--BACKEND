using API.Data;
using API.Modelo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public UsersController(UserService userService, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }



        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.FarmName = model.FarmName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User updated successfully" });
            }

            return BadRequest(new { message = "User update failed", errors = result.Errors });
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
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