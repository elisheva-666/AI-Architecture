namespace ChineseAuction.Api.Dtos
{
    /// <summary>
    /// Event published to Kafka when an order is confirmed.
    /// Topic: order-confirmed
    /// </summary>
    public class OrderConfirmedEvent
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public List<OrderConfirmedEventItem> Items { get; set; } = new();
    }

    public class OrderConfirmedEventItem
    {
        public int GiftId { get; set; }
        public string GiftName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// Event published to Kafka when a lottery draw is completed.
    /// Topic: lottery-drawn
    /// </summary>
    public class LotteryDrawnEvent
    {
        public int GiftId { get; set; }
        public string GiftName { get; set; } = string.Empty;
        public int WinnerUserId { get; set; }
        public string WinnerName { get; set; } = string.Empty;
        public string WinnerEmail { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public DateTime DrawnAt { get; set; }
    }
}
