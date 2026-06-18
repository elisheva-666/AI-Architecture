using ChineseAuction.Api.Dtos;

namespace ChineseAuction.Api.Services
{
    public interface ILotteryService
    {
        /// <summary>
        /// גורל את הזוכה עבור מתנה ספציפית לפי giftId.
        /// המחיר אינו משנה את הסתברות, הסתברות מבוססת על כמות הכרטיסים שכל משתמש קנה.
        /// מחזיר DTO של הזוכה או null אם אין כרטיסים.
        /// </summary>
        Task<WinnerResultDto?> DrawForGiftAsync(int giftId);

        /// <summary>
        /// מריץ הגרלה לכל המתנות שמכירות להן מאושרות, שומר זוכים, ויוצר דוחות בקובצי CSV.
        /// מחזיר רשימת תוצאות לכל מתנה.
        /// </summary>
        Task<IEnumerable<WinnerResultDto>> DrawAllAsync();
    }
}