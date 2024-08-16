using API.Modelo;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Data
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<bool> RegisterUser(RegisterModel model)
        {
            if (IsValidRegistrationKey(model))
            {
                // Generate a random password
                string generatedPassword = GenerateRandomPassword();

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    FarmName = model.FarmName,
                    RequirePasswordChange = true,
                    EmailConfirmed = true // Set this to true if you want to skip email confirmation
                };

                var result = await _userManager.CreateAsync(user, generatedPassword);

                if (result.Succeeded)
                {
                    // Send email with generated password
                    await _emailService.SendEmailAsync(user.Email, "Your Account Password",
                        $"Your account has been created. Your temporary password is: {generatedPassword}. Please change your password upon first login.");

                    return true;
                }
            }
            return false;
        }

        private bool IsValidRegistrationKey(RegisterModel model)
        {
            throw new NotImplementedException();
        }

        private bool IsValidRegistrationKey(string key)
        {
            var validKeys = _configuration.GetSection("ValidRegistrationKeys").Get<List<string>>();
            return validKeys.Contains(key);
        }

        private string GenerateRandomPassword()
        {
            // Implement password generation logic here
            // This is a simple example and should be made more robust in a real application
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<string> LoginUser(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return null;
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (result.Succeeded)
            {
                return GenerateJwtToken(user);
            }
            return null;
        }

        public async Task<bool> UpdateUser(string userId, UpdateUserModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            user.UserName = model.Username;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.FarmName = model.FarmName;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));
            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}