namespace API.Modelo
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string token { get; set; } // Cambiar a "token"
        public string userId { get; set; } // Cambiar a "userId"
        public bool requirePasswordChange { get; set; } // Cambiar a "requirePasswordChange"
        public string Message { get; set; }
    }

}