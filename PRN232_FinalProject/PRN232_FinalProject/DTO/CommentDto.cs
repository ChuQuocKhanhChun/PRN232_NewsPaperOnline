namespace PRN232_FinalProject.DTO
{
    public class CommentDto
    {
        public int CommentID { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ArticleID { get; set; }  
        public string? UserID { get; set; } // ApplicationUser.Id
        public string? UserFullName { get; set; } // For display
    }
}
