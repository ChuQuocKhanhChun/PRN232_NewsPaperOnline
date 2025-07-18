﻿using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
