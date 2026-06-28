using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymWeb.Models
{
    public class Member
    {
        [Key]
        public int MemberID { get; set; }
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "Nam";
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";

        // Liên kết khóa ngoại tới bảng Account
        public int AccountID { get; set; }
        [ForeignKey("AccountID")]
        public virtual Account? Account { get; set; }
    }
}