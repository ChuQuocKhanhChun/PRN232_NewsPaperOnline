namespace PRN232_FinalProject.DTO
{
    public class UserProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string? Image { get; set; }
    }
}
