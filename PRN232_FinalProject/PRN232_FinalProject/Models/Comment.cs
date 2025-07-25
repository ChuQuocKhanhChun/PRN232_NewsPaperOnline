using PRN232_FinalProject.Identity;
using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models;

public partial class Comment
{
    public int CommentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string AuthorId { get; set; } = null!; // Người dùng đã bình luận
    public int ArticleId { get; set; } // Bài viết mà bình luận thuộc về
    public int? ParentCommentId { get; set; } // Cho bình luận lồng nhau (nullable)
    public bool IsDeleted { get; set; } = false; // Xóa mềm

    // Navigation properties
    public virtual ApplicationUser Author { get; set; } = null!; // Giả định bạn có ApplicationUser
    public virtual Article Article { get; set; } = null!;
    public virtual Comment? ParentComment { get; set; } // Tự tham chiếu cho các câu trả lời
    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>(); // Cho các câu trả lời của bình luận này
}
