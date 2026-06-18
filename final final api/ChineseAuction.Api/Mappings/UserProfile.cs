using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Mappings
{
    public class UserProfile :Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDto>();

            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.PasswordHash,
                    opt => opt.Ignore());

            // UpdateDto -> Entity (לעדכון)
            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

