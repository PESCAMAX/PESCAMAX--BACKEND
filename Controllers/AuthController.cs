﻿using Microsoft.AspNetCore.Mvc;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

            string generatedPassword = GenerateRandomPassword();
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                FarmName = model.FarmName,
                EmailConfirmed = true,
                RequirePasswordChange = true
            };

            var result = await _userManager.CreateAsync(user, generatedPassword);
            if (result.Succeeded)
            {
                string emailBody = $@"
                <!DOCTYPE html>
                <html lang=""es"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Tu Cuenta ha sido Creada</title>
                    <style>
                        /* ... (mantén los estilos existentes) ... */
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Tu Cuenta ha sido Creada</h1>
                        </div>
                        <div class=""content"">
                            <h2>Bienvenido, {user.UserName}</h2>
                            <p>Tu cuenta ha sido creada exitosamente.</p>
                            <p>Tu contraseña temporal es: <strong>{generatedPassword}</strong></p>
                            <p>Por favor, cambia tu contraseña al iniciar sesión por primera vez.</p>
                            <a href=""http://localhost:4200/"" class=""button"">Iniciar Sesión</a>
                            <p>Si no solicitaste esta cuenta, por favor contacta con nuestro equipo de soporte inmediatamente.</p>
                        </div>
                        <div class=""footer"">
                            <p>Gracias por unirte a nosotros. Estamos aquí para ayudarte.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Tu Cuenta ha sido Creada",
                    emailBody
                );

                return Ok(new { success = true, message = "User registered successfully. Check your email for the temporary password." });
            }

            return BadRequest(new { success = false, errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _authService.AuthenticateUser(model.Username, model.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Generate JWT token for backward compatibility or API usage
            var token = GenerateJwtToken(user);

            return Ok(new { message = "Login successful", token = token, requirePasswordChange = user.RequirePasswordChange });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logout successful" });
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

                string emailBody = $@"
                <!DOCTYPE html>
                <html lang=""es"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Recuperar Contraseña</title>
                    <style>
                        /* ... (mantén los estilos existentes) ... */
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Recuperar Contraseña</h1>
                        </div>
                        <div class=""content"">
                            <h2>Hola {user.UserName},</h2>
                            <p>Has solicitado restablecer tu contraseña. Haz clic en el botón de abajo para proceder:</p>
                            <a href=""{callbackUrl}"" class=""button"">Restablecer Contraseña</a>
                            <p>Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña permanecerá sin cambios.</p>
                        </div>
                        <div class=""footer"">
                            <p>Este es un correo automático, por favor no responder.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(model.Email, "Restablecer contraseña", emailBody);
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

        private string GenerateRandomPassword()
        {
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string numeric = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var random = new Random();
            var password = new StringBuilder();

            // Asegúrate de incluir al menos un carácter de cada tipo
            password.Append(upperCase[random.Next(upperCase.Length)]);
            password.Append(lowerCase[random.Next(lowerCase.Length)]);
            password.Append(numeric[random.Next(numeric.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Completa el resto de la contraseña
            while (password.Length < 12)
            {
                var set = random.Next(4);
                switch (set)
                {
                    case 0:
                        password.Append(upperCase[random.Next(upperCase.Length)]);
                        break;
                    case 1:
                        password.Append(lowerCase[random.Next(lowerCase.Length)]);
                        break;
                    case 2:
                        password.Append(numeric[random.Next(numeric.Length)]);
                        break;
                    case 3:
                        password.Append(special[random.Next(special.Length)]);
                        break;
                }
            }

            return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                user.RequirePasswordChange = false;
                await _userManager.UpdateAsync(user);
                return Ok(new { success = true, message = "Password changed successfully." });
            }

            return BadRequest(new { success = false, errors = changePasswordResult.Errors });
        }
    }
}