using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using GymWeb.Models;

namespace GymWeb.Services
{
    // Gửi email thật qua SMTP (mặc định cấu hình sẵn cho Gmail).
    // Yêu cầu SenderEmail là địa chỉ Gmail và SenderPassword là "App Password" 16 ký tự
    // (Google Account > Bảo mật > Xác minh 2 bước > Mật khẩu ứng dụng), không phải mật khẩu Gmail thường.
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình EmailSettings:SenderEmail / SenderPassword (dùng dotnet user-secrets set để thêm Gmail App Password).");
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}
