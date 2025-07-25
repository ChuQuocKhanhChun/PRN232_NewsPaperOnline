using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Models;
using TagService.Grpc; // Namespace from your .proto file

public class TagGrpcService : TagService.Grpc.TagService.TagServiceBase
{
    private readonly Prn232FinalProjectContext _dbContext;
    private readonly ILogger<TagGrpcService> _logger;

    public TagGrpcService(Prn232FinalProjectContext dbContext, ILogger<TagGrpcService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override async Task<TagResponse> CreateTag(CreateTagRequest request, ServerCallContext context)
    {
        var tag = new Tag { Name = request.Name };
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        return new TagResponse { Tag = new TagModel { TagId = tag.TagId, Name = tag.Name } };
    }

    public override async Task<TagResponse> GetTag(GetTagRequest request, ServerCallContext context)
    {
        var tag = await _dbContext.Tags.FindAsync(request.TagId);
        if (tag == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Tag with ID {request.TagId} not found."));
        }

        return new TagResponse { Tag = new TagModel { TagId = tag.TagId, Name = tag.Name } };
    }

    public override async Task<GetAllTagsResponse> GetAllTags(GetAllTagsRequest request, ServerCallContext context)
    {
        var tags = await _dbContext.Tags.ToListAsync();
        var response = new GetAllTagsResponse();
        response.Tags.AddRange(tags.Select(t => new TagModel { TagId = t.TagId, Name = t.Name }));
        return response;
    }

    public override async Task<TagResponse> UpdateTag(UpdateTagRequest request, ServerCallContext context)
    {
        var tag = await _dbContext.Tags.FindAsync(request.TagId);
        if (tag == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Tag with ID {request.TagId} not found."));
        }

        tag.Name = request.Name;
        await _dbContext.SaveChangesAsync();

        return new TagResponse { Tag = new TagModel { TagId = tag.TagId, Name = tag.Name } };
    }

    public override async Task<DeleteTagResponse> DeleteTag(DeleteTagRequest request, ServerCallContext context)
    {
        var tag = await _dbContext.Tags.FindAsync(request.TagId);
        if (tag == null)
        {
            return new DeleteTagResponse { Success = false, Message = $"Tag with ID {request.TagId} not found." };
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync();

        return new DeleteTagResponse { Success = true, Message = $"Tag with ID {request.TagId} deleted successfully." };
    }
}