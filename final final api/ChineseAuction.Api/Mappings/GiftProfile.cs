using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Mappings
{
    public class GiftProfile : Profile
    {
        public GiftProfile()
        {
            // 1. מיפוי בסיסי מה-Entity ל-DTOs (קריאה)
            CreateMap<Gift, GiftDto>();

            // 2. מיפוי מפורט - AutoMapper יודע לזהות לבד ש-CategoryName מגיע מ-Category.Name
            // אבל כדאי להגדיר זאת מפורשות אם השמות שונים במעט.
            CreateMap<Gift, GiftDetailDto>()
             .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
             .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => src.Donor != null ? src.Donor.Name : null));

            // 3. מיפוי למנהל - כולל חישוב כמות רוכשים מהרשימה OrderItems
            CreateMap<Gift, GiftAdminDto>()
                 .IncludeBase<Gift, GiftDetailDto>()
                 .ForMember(dest => dest.PurchasersCount,
                   opt => opt.MapFrom(src => src.OrderItems != null ? src.OrderItems.Sum(oi => oi.Quantity) : 0))
                 .ForMember(dest => dest.DonorEmail, opt => opt.MapFrom(src => src.Donor != null ? src.Donor.Email : null));

            // 4. מיפוי מה-DTO ל-Entity (כתיבה/עדכון)
            CreateMap<GiftCreateUpdateDto, Gift>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore()); // מונע שגיאת Identity
        }
    }
}

