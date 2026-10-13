using Auth_System.Models.Entity;

namespace Auth_System.Models
{
    public class PendingRegistration
    {
        public User User { get; set; } = new();
        public string TokenHash { get; set; } = string.Empty;
        public int Attempts { get; set; } = 0;
    }
}
