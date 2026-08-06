namespace GymWeb.Models
{
    // Cấu hình SMTP để gửi email (OTP quên mật khẩu...). Giá trị thật nằm trong
    // dotnet user-secrets (SenderEmail / SenderPassword = Gmail App Password), KHÔNG lưu trong repo.
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderName { get; set; } = "GYM FITNESS";
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
    }
}
