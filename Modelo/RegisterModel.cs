﻿namespace API.Modelo
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string FarmName { get; set; }
        public string RegistrationKey { get; set; }  // Propiedad para la clave de registro

    }
}