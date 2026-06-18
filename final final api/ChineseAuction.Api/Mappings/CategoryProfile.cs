using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();

        }
    }
}
