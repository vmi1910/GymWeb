namespace GymWeb.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int TotalMembers { get; set; }
        public int ActivePackages { get; set; }
        public int TotalStaff { get; set; }
        public int TotalTrainers { get; set; }

        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();
    }
}
