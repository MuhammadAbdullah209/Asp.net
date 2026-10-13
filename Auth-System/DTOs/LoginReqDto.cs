using System.ComponentModel.DataAnnotations;

namespace Auth_System.DTOs
{
    public class LoginReqDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
