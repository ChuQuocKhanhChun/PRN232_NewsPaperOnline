namespace PRN232_FinalProject.DTO
{
    public class ArticleDto
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Status { get; set; } // Published, Draft, Archived, Pending
        public DateTime CreatedAt { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // optional, useful for display

        public List<int> TagIds { get; set; } = new();
        public List<string> TagNames { get; set; } = new();
    }


}
