using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class IncomeModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public DateTime IncomeDate { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        public UserModel? User { get; set; }
    }
}