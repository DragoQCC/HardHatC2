namespace TeamServer.Models.Database
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? PasswordHash { get; set; }
    }
}
