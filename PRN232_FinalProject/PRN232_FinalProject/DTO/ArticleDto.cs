namespace PRN232_FinalProject.DTO
{
    public class ArticleDto
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string? Status { get; set; }
        public int AuthorId { get; set; } 
        public int CategoryId { get; set; }
    }

}
