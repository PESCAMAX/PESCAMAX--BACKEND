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
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public bool RequirePasswordChange { get; set; }
        public string Message { get; set; }
    }
}