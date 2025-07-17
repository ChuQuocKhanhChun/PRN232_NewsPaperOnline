using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.DTO
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
