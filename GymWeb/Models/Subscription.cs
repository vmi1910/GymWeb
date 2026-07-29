using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Một lượt đăng ký gói tập của hội viên
    public class Subscription
    {
        [Key]
        public int SubscriptionID { get; set; }

        [Required]
        public int MemberID { get; set; }
        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }

        [Required]
        public int PackageID { get; set; }
        [ForeignKey("PackageID")]
        public virtual MembershipPackage? Package { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        public decimal TotalAmount { get; set; }
    }
}
