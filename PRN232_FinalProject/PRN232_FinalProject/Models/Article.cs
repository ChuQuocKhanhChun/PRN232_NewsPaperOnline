using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class Article
{
    public int ArticleId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public DateTime? PublishedDate { get; set; }

    public string Status { get; set; } = null!;

    public int? Views { get; set; }

    public int AuthorId { get; set; }

    public int CategoryId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ArticleHistory> ArticleHistories { get; set; } = new List<ArticleHistory>();

    public virtual ICollection<ArticleLike> ArticleLikes { get; set; } = new List<ArticleLike>();

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
