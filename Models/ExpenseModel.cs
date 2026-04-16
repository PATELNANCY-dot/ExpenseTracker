namespace ExpenseTracker.Models
{
    public class ExpenseModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int UserId { get; set; }
        public string Notes { get; set; }
    }
}
