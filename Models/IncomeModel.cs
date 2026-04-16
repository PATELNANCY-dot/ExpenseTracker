namespace ExpenseTracker.Models
{
    public class IncomeModel
    {
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime IncomeDate { get; set; }
        public int UserId { get; set; }
    }
}
