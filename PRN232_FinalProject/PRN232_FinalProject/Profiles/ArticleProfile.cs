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
            .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.Tags.Select(t => t.TagId)))
            .ForMember(dest => dest.TagNames, opt => opt.MapFrom(src => src.Tags.Select(t => t.Name)))
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Views))
            .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.ArticleLikes.Count()))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ReverseMap()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore());


            // Category ↔ CategoryDto
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Tag ↔ TagDto
            CreateMap<Tag, TagDto>().ReverseMap();

            // Comment ↔ CommentDto
           

            

            // RegisterDto → ApplicationUser (for registration)
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }

}
