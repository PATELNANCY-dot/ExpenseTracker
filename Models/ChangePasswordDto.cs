using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class ChangePasswordDto
    {
        public int Id { get; set; }

        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}