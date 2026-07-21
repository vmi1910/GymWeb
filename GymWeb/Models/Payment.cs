using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    // Hóa đơn / lần thanh toán gắn với một Subscription
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int SubscriptionID { get; set; }
        [ForeignKey("SubscriptionID")]
        public virtual Subscription? Subscription { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(12,0)")]
        public decimal Amount { get; set; }

        public string Method { get; set; } = "Tiền mặt";

        // Nhân viên thu ngân xử lý (có thể để trống)
        public int? StaffID { get; set; }
        [ForeignKey("StaffID")]
        public virtual Staff? Staff { get; set; }
    }
}
