namespace Auth_System.DTOs
{
    public class LoginResDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public static LoginResDto Success(string name, string email, string token)
        {
            return new LoginResDto
            {
                Name = name,
                Email = email,
                Token = token
            };
        }

        public static LoginResDto Failure()
        {
            return new LoginResDto();
        }
    }
}
