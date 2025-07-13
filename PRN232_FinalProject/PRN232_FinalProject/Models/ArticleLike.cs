using PRN232_FinalProject.Identity;
using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class ArticleLike
{
    public int ArticleId { get; set; }

    public string UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual ApplicationUser User { get; set; }
}
