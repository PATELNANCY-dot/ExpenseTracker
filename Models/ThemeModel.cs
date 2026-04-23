namespace ExpenseTracker.Models
{
    public class ThemeModel
    {
        public int? Id { get; set; }     
        public int UserId { get; set; }
        public bool DarkMode { get; set; }
    }
}