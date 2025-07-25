namespace PRN232_FinalProject_Client.DTO
{
    public class CommentDto
    {
        public string Content { get; set; } = null!;
        public string AuthorId { get; set; } = null!;
        public int ArticleId { get; set; }  
        public int? ParentCommentId { get; set; }
    }
}
