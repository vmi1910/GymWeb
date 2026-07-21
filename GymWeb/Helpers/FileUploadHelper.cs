using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GymWeb.Helpers
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        // Lưu file ảnh vào wwwroot/uploads/{subFolder}/ và trả về đường dẫn tương đối (VD: /uploads/certificates/xxx.jpg)
        // Trả về null nếu file không hợp lệ; errorMessage sẽ chứa lý do.
        public static string? SaveImage(IWebHostEnvironment env, IFormFile file, string subFolder, out string? errorMessage)
        {
            errorMessage = null;

            if (file.Length <= 0)
            {
                errorMessage = "File rỗng, vui lòng chọn lại.";
                return null;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                errorMessage = "Ảnh vượt quá dung lượng cho phép (tối đa 5MB).";
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, webp, gif).";
                return null;
            }

            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString("N") + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/" + subFolder + "/" + fileName;
        }

        // Xóa file cũ (nếu có) khi thay ảnh mới, bỏ qua lỗi nếu file không tồn tại
        public static void DeleteIfExists(IWebHostEnvironment env, string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var fullPath = Path.Combine(env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
            {
                try { File.Delete(fullPath); } catch { /* bỏ qua lỗi xóa file */ }
            }
        }
    }
}
