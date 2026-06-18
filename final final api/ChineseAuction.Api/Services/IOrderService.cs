using ChineseAuction.Api.Dtos;

namespace ChineseAuction.Api.Services
{
    public interface IOrderService
    {
        Task<bool> ConfirmOrderAsync(int id);
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto);
        Task<bool> DeleteOrderAsync(int orderId, int userId);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
        Task<IEnumerable<GiftPurchasesDto>> GetByGiftIdAsync(int giftId);
        Task<OrderResponseDto?> GetLatestDraftOrderAsync(int userId);
        Task<OrderResponseDto?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<GiftPurchasesDto>> GetPurchasesByGiftsAsync();
        Task<bool> DeleteOrderItemAsync(int orderId, int orderItemId, int userId);
    }
}