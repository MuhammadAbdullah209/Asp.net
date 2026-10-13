using System.ComponentModel.DataAnnotations;

namespace Auth_System.DTOs
{
    public class ResgisterReqDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DOB { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;
        [RegularExpression(
       @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
       ErrorMessage = "Please enter a valid email address."
   )]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone Number Is Required!")]
        [RegularExpression(
    @"^(03\d{9}|\+923\d{9}|00923\d{9})$",
    ErrorMessage = "Enter a valid Pakistani phone number, e.g. 03001234567 or +923001234567."
)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
