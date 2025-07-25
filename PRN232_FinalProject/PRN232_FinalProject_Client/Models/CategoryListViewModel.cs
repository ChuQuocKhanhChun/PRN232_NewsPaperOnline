using CategoryService.Grpc;

namespace PRN232_FinalProject_Client.Models
{
    public class CategoryListViewModel
    {
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
    }
}
