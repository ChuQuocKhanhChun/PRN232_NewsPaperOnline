using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ArticleHistory> ArticleHistories { get; set; } = new List<ArticleHistory>();

    public virtual ICollection<ArticleLike> ArticleLikes { get; set; } = new List<ArticleLike>();

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
