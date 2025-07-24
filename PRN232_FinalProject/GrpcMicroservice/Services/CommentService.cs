using Grpc.Core;
using GrpcCommentService;
using PRN232_FinalProject.Models;

namespace GrpcMicroservice.Services
{
    public class CommentService : GrpcCommentService.CommentService.CommentServiceBase
    {
        private readonly Prn232FinalProjectContext _context;

        public CommentService(Prn232FinalProjectContext context)
        {
            _context = context;
        }

        public override async Task<CommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
        {
            var comment = new Comment
            {
                Content = request.Content,
                UserId = request.UserId,
                CommentDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentResponse
            {
                Id = comment.CommentId.ToString(),
                Content = comment.Content,
                UserId = comment.UserId,
                CreatedAt = comment.CommentDate.HasValue ? comment.CommentDate.Value.ToString("o") : string.Empty
            };
        }

        public override async Task<DeleteCommentResult> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
        {
            var comment = await _context.Comments.FindAsync(request.CommentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return new DeleteCommentResult { Success = true };
            }
            return new DeleteCommentResult { Success = false };
        }
    }
}
