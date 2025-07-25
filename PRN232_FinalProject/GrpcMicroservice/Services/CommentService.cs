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

       
    }
}
