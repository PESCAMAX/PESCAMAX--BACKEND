using Microsoft.AspNetCore.Identity;

namespace API.Modelo
{
    public class ApplicationUser : IdentityUser
    {
        public string Token { get; internal set; }
    }
}
