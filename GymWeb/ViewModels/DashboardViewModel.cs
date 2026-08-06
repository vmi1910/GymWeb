namespace GymWeb.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public int TotalMembers { get; set; }
        public int NewMembersThisMonth { get; set; }
        public int ActivePackages { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalStaff { get; set; }
        public int TotalTrainers { get; set; }

        // % thay đổi doanh thu tháng này so với tháng trước (null nếu tháng trước không có dữ liệu để so sánh)
        public double? RevenueTrendPercent { get; set; }

        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();

        public List<string> PackagePopularityLabels { get; set; } = new();
        public List<int> PackagePopularityCounts { get; set; } = new();

        public List<RecentPaymentItem> RecentPayments { get; set; } = new();
    }

    public class RecentPaymentItem
    {
        public string MemberName { get; set; } = "—";
        public string PackageName { get; set; } = "—";
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Method { get; set; } = "";
    }
}
