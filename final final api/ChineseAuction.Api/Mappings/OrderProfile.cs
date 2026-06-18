using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;

namespace ChineseAuction.Api.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderCreateDto, Order>();
            CreateMap<OrderItemCreateDto, OrderItem>();

            CreateMap<Order, OrderResponseDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.Name))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            CreateMap<OrderItem, OrderItemResponseDto>()
                .ForMember(d => d.GiftName, o => o.MapFrom(s => s.Gift.Name))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Gift.TicketPrice));

            // ה-OrderItem מכיל בתוכו את ה-Order, שדרכו מגיעים ל-User
            CreateMap<OrderItem, BuyerDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.Order.User.Name))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Order.User.Email))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));
        }
    }
}
