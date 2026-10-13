using System.ComponentModel.DataAnnotations;

namespace Auth_System.Models.Entity
{
    public class User
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DOB { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage ="Phone Number Is Required!")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }
}
