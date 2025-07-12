using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class ArticleHistory
{
    public int HistoryId { get; set; }

    public int ArticleId { get; set; }

    public int EditedBy { get; set; }

    public DateTime? EditedDate { get; set; }

    public string? OldTitle { get; set; }

    public string? OldContent { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual User EditedByNavigation { get; set; } = null!;
}
