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

        public string Description { get; set; } = string.Empty;

        // Gói đang được bán hay đã ngừng áp dụng
        public bool IsActive { get; set; } = true;
    }
}
