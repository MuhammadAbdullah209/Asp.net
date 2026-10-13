using System.ComponentModel.DataAnnotations;

namespace Auth_System.DTOs
{
    public class ForgotPasswordReqDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}