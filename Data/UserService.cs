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

        public UserService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<bool> RegisterUser(RegisterModel model)
        {
            if (IsValidRegistrationKey(model.RegistrationKey))
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email, // Use email as username
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    FarmName = model.FarmName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                return result.Succeeded;
            }

            return false;
        }

        private bool IsValidRegistrationKey(string key)
        {
            var validKeys = _configuration.GetSection("ValidRegistrationKeys").Get<List<string>>();
            return validKeys.Contains(key);
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
            // Aquí implementarías la lógica para generar un token JWT
            // Este es solo un ejemplo básico
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
