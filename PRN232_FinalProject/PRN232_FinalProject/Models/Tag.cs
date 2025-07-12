using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class Tag
{
    public int TagId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
