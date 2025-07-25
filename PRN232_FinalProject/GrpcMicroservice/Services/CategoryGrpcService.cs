// CategoryService.Grpc/Services/CategoryGrpcService.cs
using CategoryService.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Models;

public class CategoryGrpcService : CategoryService.Grpc.CategoryService.CategoryServiceBase
{
    private readonly Prn232FinalProjectContext _dbContext;
    private readonly ILogger<CategoryGrpcService> _logger;

    public CategoryGrpcService(Prn232FinalProjectContext dbContext, ILogger<CategoryGrpcService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override async Task<CategoryResponse> CreateCategory(CreateCategoryRequest request, ServerCallContext context)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true // Mặc định là Active khi tạo mới
        };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Đã tạo category mới: {CategoryId}", category.CategoryId);
        return new CategoryResponse
        {
            Category = new CategoryModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description ?? "",
                IsActive = category.IsActive ?? false
            }
        };
    }

    public override async Task<CategoryResponse> GetCategory(GetCategoryRequest request, ServerCallContext context)
    {
        var category = await _dbContext.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Không tìm thấy category với ID: {CategoryId}", request.CategoryId);
            throw new RpcException(new Status(StatusCode.NotFound, $"Category with ID {request.CategoryId} not found."));
        }

        _logger.LogInformation("Đã lấy category: {CategoryId}", category.CategoryId);
        return new CategoryResponse
        {
            Category = new CategoryModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description ?? "",
                IsActive = category.IsActive ?? false
            }
        };
    }

    public override async Task<GetAllCategoriesResponse> GetAllCategories(GetAllCategoriesRequest request, ServerCallContext context)
    {
        var categories = await _dbContext.Categories.ToListAsync();
        var response = new GetAllCategoriesResponse();
        response.Categories.AddRange(categories.Select(c => new CategoryModel
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Description = c.Description ?? "",
            IsActive = c.IsActive ?? false
        }));
        _logger.LogInformation("Đã lấy tất cả {Count} categories.", categories.Count);
        return response;
    }

    public override async Task<CategoryResponse> UpdateCategory(UpdateCategoryRequest request, ServerCallContext context)
    {
        var category = await _dbContext.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Không tìm thấy category với ID: {CategoryId} để cập nhật.", request.CategoryId);
            throw new RpcException(new Status(StatusCode.NotFound, $"Category with ID {request.CategoryId} not found."));
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive; // Cập nhật trạng thái
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Đã cập nhật category: {CategoryId}", category.CategoryId);
        return new CategoryResponse
        {
            Category = new CategoryModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description ?? "",
                IsActive = category.IsActive ?? false
            }
        };
    }

    public override async Task<DeleteCategoryResponse> DeleteCategory(DeleteCategoryRequest request, ServerCallContext context)
    {
        var category = await _dbContext.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Không tìm thấy category với ID: {CategoryId} để xóa.", request.CategoryId);
            return new DeleteCategoryResponse { Success = false, Message = $"Category with ID {request.CategoryId} not found." };
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Đã xóa category: {CategoryId}", category.CategoryId);
        return new DeleteCategoryResponse { Success = true, Message = $"Category with ID {request.CategoryId} deleted successfully." };
    }

    public override async Task<CategoryResponse> ToggleCategoryActiveStatus(ToggleCategoryActiveStatusRequest request, ServerCallContext context)
    {
        var category = await _dbContext.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Không tìm thấy category với ID: {CategoryId} để chuyển đổi trạng thái.", request.CategoryId);
            throw new RpcException(new Status(StatusCode.NotFound, $"Category with ID {request.CategoryId} not found."));
        }

        category.IsActive = !(category.IsActive ?? false); // Đảo ngược trạng thái hiện tại
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Đã chuyển đổi trạng thái category {CategoryId} sang IsActive: {IsActive}", category.CategoryId, category.IsActive);
        return new CategoryResponse
        {
            Category = new CategoryModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description ?? "",
                IsActive = category.IsActive ?? false
            }
        };
    }
}