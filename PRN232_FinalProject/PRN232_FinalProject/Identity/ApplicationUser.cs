using Microsoft.AspNetCore.Identity;

namespace PRN232_FinalProject.Identity
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Image { get; set; }
    }
}
