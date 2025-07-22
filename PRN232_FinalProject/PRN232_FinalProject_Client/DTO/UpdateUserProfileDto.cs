using Microsoft.AspNetCore.Mvc;

namespace PRN232_FinalProject_Client.DTO
{
    public class UpdateUserProfileDto
    {
        public string? FullName { get; set; }

        [FromForm(Name = "ImageFile")]
        public IFormFile? ImageFile { get; set; }
    }
}
