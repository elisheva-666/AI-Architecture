using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Mappings
{
    public class DonorProfile : Profile
    {
        public DonorProfile()
        {
            CreateMap<Donor, DonorDto>();
            CreateMap<Donor, DonorDetailDto>();
            CreateMap<DonorUpdateDto, Donor>();


            CreateMap<Gift, GiftDto>();

            CreateMap<GiftCreateUpdateDto, Gift>();

            CreateMap<DonorCreateDto, Donor>()
                .ForMember(dest => dest.Gifts, opt => opt.MapFrom(src => src.NewGifts));

        }
    }
}
