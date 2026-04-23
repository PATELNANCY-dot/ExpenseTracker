using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class ExpenseModel
    {
        public int? Id { get; set; }

        [Required]
        public string? Title { get; set; } = string.Empty;

        [Required]
        public decimal? Amount { get; set; }

        [Required]
        public string? Category { get; set; } = string.Empty;

        public DateTime? ExpenseDate { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        public string? Notes { get; set; }

        public UserModel? User { get; set; }
    }
}