using PRN232_FinalProject.Identity;
using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CommentDate { get; set; }

    public bool? IsApproved { get; set; }

    public string UserId { get; set; }

    public int ArticleId { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual ApplicationUser User { get; set; }
}
