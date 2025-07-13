namespace PRN232_FinalProject.DTO
{
    public class ArticleDto2
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string? Status { get; set; }
        public string AuthorId { get; set; }
        public int CategoryId { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
