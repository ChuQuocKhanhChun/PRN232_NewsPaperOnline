namespace PRN232_FinalProject_Client.DTO
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }

        // Add these properties for filtering
        public List<string> TagNames { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
