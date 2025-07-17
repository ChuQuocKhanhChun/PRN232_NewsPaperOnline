namespace PRN232_FinalProject.DTO
{
    public class UserResetDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

    }
}
