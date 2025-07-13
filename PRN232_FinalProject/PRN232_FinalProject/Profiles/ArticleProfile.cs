using AutoMapper;
using AutoMapper.Features;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Identity;
using PRN232_FinalProject.Models;

namespace PRN232_FinalProject.Profiles
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            // Article ↔ ArticleDto
            CreateMap<Article, ArticleDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ReverseMap()
                .ForMember(dest => dest.Category, opt => opt.Ignore()); // Avoid circular reference

            // Category ↔ CategoryDto
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Tag ↔ TagDto
            CreateMap<Tag, TagDto>().ReverseMap();

            // Comment ↔ CommentDto
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            

            // RegisterDto → ApplicationUser (for registration)
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }

}
