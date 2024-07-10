using API.Modelo;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace API.Data
{
    public interface IAuthService
    {
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result;
        }

    }
}