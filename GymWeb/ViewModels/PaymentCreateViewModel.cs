using System.ComponentModel.DataAnnotations;

namespace GymWeb.ViewModels
{
    // Dùng khi lập hóa đơn: chọn hội viên + gói tập -> tự tạo Subscription + Payment
    public class PaymentCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn hội viên")]
        public int MemberID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn gói tập")]
        public int PackageID { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ")]
        public decimal Amount { get; set; }

        public string Method { get; set; } = "Tiền mặt";
    }
}
