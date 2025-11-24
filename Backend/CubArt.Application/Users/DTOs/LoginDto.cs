namespace CubArt.Application.Users.DTOs
{
    public class LoginDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
