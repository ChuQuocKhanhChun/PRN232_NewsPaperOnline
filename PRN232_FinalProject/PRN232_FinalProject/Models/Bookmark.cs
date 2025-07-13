using PRN232_FinalProject.Identity;
using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class Bookmark
{
    public string UserId { get; set; }

    public int ArticleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual ApplicationUser User { get; set; }
}
