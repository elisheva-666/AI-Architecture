namespace ChineseAuction.Api.Dtos
{
    // תוצאה של הגרלה עבור מתנה אחת
    public class WinnerResultDto
    {
        public int GiftId { get; set; }
        public string GiftName { get; set; } = null!;
        public int WinnerUserId { get; set; }
        public string WinnerName { get; set; } = null!;
        public string? WinnerEmail { get; set; }
        public int TotalTickets { get; set; }
        public DateTime DrawDate { get; set; }
    }
}