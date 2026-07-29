namespace GymWeb.ViewModels
{
    // Dùng cho trang "Quản lý tài khoản" - gộp thêm thông tin để nút Sửa trỏ đúng
    // trang chỉnh sửa hồ sơ tương ứng (Member / Staff / Trainer) theo Role của tài khoản.
    public class AccountListItemViewModel
    {
        public int AccountID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // Controller và Id dùng để dựng link "Sửa" (rỗng nếu chưa có hồ sơ liên kết)
        public string EditController { get; set; } = string.Empty;
        public int EditId { get; set; }
    }
}
