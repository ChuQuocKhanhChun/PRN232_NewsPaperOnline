using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject.DTO
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
