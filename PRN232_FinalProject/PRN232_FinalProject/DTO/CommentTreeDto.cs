namespace PRN232_FinalProject.DTO
{
    public class CommentTreeDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImage { get; set; }  // optional
        public bool IsDeleted { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentTreeDto> Replies { get; set; } = new();
    }
}
