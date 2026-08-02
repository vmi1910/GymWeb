using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    public class MembershipPackage
    {
        [Key]
        public int PackageID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên gói tập")]
        public string PackageName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số ngày hiệu lực phải lớn hơn 0")]
        public int DurationDays { get; set; }

        [Column(TypeName = "decimal(12,0)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá gói tập không hợp lệ")]
        public decimal Price { get; set; }

        // Mô tả ngắn gọn, hiển thị dưới tên gói
        public string Description { get; set; } = string.Empty;

        // Đường dẫn ảnh minh họa (VD: /images/packages/gold.jpg)
        public string ImageUrl { get; set; } = "/images/packages/default.jpg";

        // Danh sách quyền lợi, mỗi dòng cách nhau bởi dấu ";"
        // VD: "Tập luyện không giới hạn;Miễn phí tủ đồ;Đo chỉ số InBody;PT hướng dẫn buổi đầu"
        public string Highlights { get; set; } = string.Empty;

        // Nhãn nổi bật, để trống nếu không cần (VD: "Phổ biến nhất", "Tiết kiệm nhất")
        public string? BadgeText { get; set; }

        // Gói được làm nổi bật (phóng to + viền đỏ)
        public bool IsFeatured { get; set; } = false;

        // Gói đang được bán hay đã ngừng áp dụng
        public bool IsActive { get; set; } = true;
    }
}