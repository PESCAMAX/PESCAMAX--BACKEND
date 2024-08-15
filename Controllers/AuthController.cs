using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using API.Modelo;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using API.Data;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService,
            IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar la clave de registro
            if (model.RegistrationKey != _configuration["RegistrationKey"])
            {
                return BadRequest(new { success = false, message = "Invalid registration key" });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                FarmName = model.FarmName,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { success = true, message = "User registered successfully" });
            }

            return BadRequest(new { success = false, errors = result.Errors });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                return Ok(new
                {
                    success = true,
                    token = token,
                    userId = user.Id,
                    username = user.UserName,
                    message = "Login successful"
                });
            }

            return Unauthorized(new { success = false, message = "Invalid email or password" });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return BadRequest(new { message = "El usuario no existe o no está confirmado." });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = $"http://localhost:4200/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

                var message = $"Por favor restablezca su contraseña haciendo clic en el siguiente enlace: <a href='{callbackUrl}'>link</a>";
                await _emailService.SendEmailAsync(model.Email, "Restablecer contraseña", message);

                return Ok(new { message = "Correo de restablecimiento de contraseña enviado." });
            }

            return BadRequest(new { message = "Solicitud inválida." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Solicitud inválida.", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { message = "El usuario no existe." });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return Ok(new { message = "Contraseña restablecida exitosamente." });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                }
            }

            return BadRequest(new { message = "No se pudo restablecer la contraseña.", errors = result.Errors });
        }
    }
}











