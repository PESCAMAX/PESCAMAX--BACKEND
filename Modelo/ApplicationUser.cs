using Microsoft.AspNetCore.Identity;

namespace API.Modelo
{
    public class ApplicationUser : IdentityUser
    {
        public string Token { get; internal set; }
        public string Address { get; set; }
        public string FarmName { get; set; }
        public bool RequirePasswordChange { get; set; }

        
    }
}

