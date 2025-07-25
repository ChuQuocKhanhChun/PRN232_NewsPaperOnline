using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using System.Security.Claims;

namespace PRN232_FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly Prn232FinalProjectContext _context;

        public CommentController(Prn232FinalProjectContext context)
        {
            _context = context;
        }

        // GET: api/Comment/tree?articleId={articleId}
        // Lấy tất cả bình luận của một bài viết, cấu trúc theo cây
        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<CommentTreeDto>>> GetCommentsTreeByArticle(int articleId)
        {
            if (_context.Comments == null)
            {
                return NotFound();
            }

            // Lấy tất cả bình luận chưa bị xóa mềm của bài viết và include thông tin tác giả
            var allComments = await _context.Comments
                .Where(c => c.ArticleId == articleId && !c.IsDeleted)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            if (!allComments.Any())
            {
                return Ok(new List<CommentTreeDto>());
            }

            // Map commentId → CommentTreeDto
            var commentMap = allComments.ToDictionary(c => c.CommentId, c => new CommentTreeDto
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                AuthorId = c.AuthorId,
                AuthorName = c.Author?.UserName ?? "Unknown",
                AuthorImage = c.Author?.Image ?? "/images/user.png", // fallback nếu không có
                IsDeleted = c.IsDeleted,
                ParentCommentId = c.ParentCommentId,
                Replies = new List<CommentTreeDto>()
            });

            var rootComments = new List<CommentTreeDto>();

            // Xây cây bình luận
            foreach (var commentDto in commentMap.Values)
            {
                if (commentDto.ParentCommentId.HasValue &&
                    commentMap.TryGetValue(commentDto.ParentCommentId.Value, out var parent))
                {
                    parent.Replies.Add(commentDto);
                }
                else
                {
                    rootComments.Add(commentDto);
                }
            }

            // Sắp xếp đệ quy theo thời gian tạo (mới nhất trước)
            rootComments = rootComments.OrderByDescending(c => c.CreatedAt).ToList();
            foreach (var root in rootComments)
            {
                SortReplies(root.Replies);
            }

            return Ok(rootComments);
        }

        private void SortReplies(List<CommentTreeDto> replies)
        {
            replies.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt)); // từ mới → cũ
            foreach (var reply in replies)
            {
                SortReplies(reply.Replies);
            }
        }



        // POST: api/Comment
        // Tạo một bình luận mới
        // [Authorize] // Yêu cầu người dùng phải đăng nhập để tạo bình luận
        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(CommentDto commentDto)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments' is null.");
            }

            // Lấy AuthorId từ Claims của người dùng đã đăng nhập (nếu đã xác thực)
            // string? authorId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy User ID từ JWT/Cookie
            // if (string.IsNullOrEmpty(authorId))
            // {
            //     return Unauthorized("User is not authenticated.");
            // }

            // Đối với mục đích test, tạm thời gán AuthorId cố định hoặc để client gửi lên (không nên dùng trong production)
            // Giả sử AuthorId được gửi từ client hoặc lấy từ JWT
            string authorId = commentDto.AuthorId; // Nếu lấy từ JWT, không cần gửi từ client

            var article = await _context.Articles.FindAsync(commentDto.ArticleId);
            if (article == null)
            {
                return BadRequest("Article not found.");
            }

            Comment comment = new Comment
            {
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow, // Sử dụng UTC để nhất quán
                AuthorId = authorId,
                ArticleId = commentDto.ArticleId,
                ParentCommentId = commentDto.ParentCommentId,
                IsDeleted = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Trả về bình luận vừa tạo cùng với ID mới
            return CreatedAtAction("GetComment", new { id = comment.CommentId }, comment);
        }

        // PUT: api/Comment/{id}
        // Cập nhật nội dung bình luận
        // [Authorize] // Yêu cầu người dùng phải đăng nhập để cập nhật bình luận
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, CommentUpdateDto commentDto)
        {
            if (id != commentDto.CommentId)
            {
                return BadRequest("Comment ID mismatch.");
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.IsDeleted)
            {
                return NotFound();
            }

            // string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (comment.AuthorId != currentUserId)
            // {
            //     // return Forbid(); // Người dùng không có quyền sửa bình luận của người khác
            // }

            comment.Content = commentDto.Content;
            // Không cập nhật CreatedAt
            // Cập nhật UpdatedAt nếu bạn thêm trường này vào model

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Trả về 204 No Content khi cập nhật thành công
        }

        // DELETE: api/Comment/{id}
        // Xóa mềm bình luận
        // [Authorize] // Yêu cầu người dùng phải đăng nhập để xóa bình luận
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (_context.Comments == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.IsDeleted)
            {
                return NotFound();
            }

            // Xóa mềm bình luận cha
            comment.IsDeleted = true;

            // Đệ quy xóa mềm các bình luận con
            await SoftDeleteChildCommentsAsync(comment.CommentId);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task SoftDeleteChildCommentsAsync(int parentCommentId)
        {
            var childComments = await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
                .ToListAsync();

            foreach (var child in childComments)
            {
                child.IsDeleted = true;

                // Gọi đệ quy tiếp nếu comment con cũng có con
                await SoftDeleteChildCommentsAsync(child.CommentId);
            }
        }

        private bool CommentExists(int id)
        {
            return (_context.Comments?.Any(e => e.CommentId == id && !e.IsDeleted)).GetValueOrDefault();
        }
    }

    // Vẫn giữ nguyên các DTO đã định nghĩa trước đó cho POST/PUT
    public class CommentDto
    {
        public string Content { get; set; } = null!;
        public string AuthorId { get; set; } = null!; // Trong thực tế, cái này nên được lấy từ JWT
        public int ArticleId { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class CommentUpdateDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = null!;
    }
}