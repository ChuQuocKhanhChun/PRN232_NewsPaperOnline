using AutoMapper;
using AutoMapper.Features;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;

namespace PRN232_FinalProject.Profiles
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>().ReverseMap();
        }
    }

}
