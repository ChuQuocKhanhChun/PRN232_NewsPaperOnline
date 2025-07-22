using Microsoft.AspNetCore.Mvc;

namespace PRN232_FinalProject_Client.DTO
{
    public class UserProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }  // Chỉ hiển thị, không cần gửi lên API
        public string? Image { get; set; }
    }

}
