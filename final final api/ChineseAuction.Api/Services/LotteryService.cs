using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace ChineseAuction.Api.Services
{
    public class LotteryService : ILotteryService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IGiftRepository _giftRepo;
        private readonly ILotteryRepository _lotteryRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<LotteryService> _logger;
        private readonly string _reportsPath;
        private readonly IEmailSender _emailSender;
        private readonly IKafkaProducer _kafka;
        private readonly string _lotteryTopic;

        public LotteryService(
            IOrderRepository orderRepo,
            IGiftRepository giftRepo,
            ILotteryRepository lotteryRepo,
            IUserRepository userRepo,
            IMapper mapper,
            ILogger<LotteryService> logger,
            IWebHostEnvironment env,
            IEmailSender emailSender,
            IKafkaProducer kafka,
            IConfiguration config)
        {
            _orderRepo    = orderRepo;
            _giftRepo     = giftRepo;
            _lotteryRepo  = lotteryRepo;
            _userRepo     = userRepo;
            _mapper       = mapper;
            _logger       = logger;
            _emailSender  = emailSender;
            _kafka        = kafka;
            _lotteryTopic = config["Kafka:LotteryDrawnTopic"] ?? "lottery-drawn";

            _reportsPath = Path.Combine(env.ContentRootPath, "Reports");
            if (!Directory.Exists(_reportsPath)) Directory.CreateDirectory(_reportsPath);
        }

        /// <summary>
        /// ���� ���� ���� ���� ��� - ����� �� ������ �������. �� ��� ������� ����� null.
        /// ���� �� �-Winner ���� � ����� ����� ����� Winners.csv
        /// </summary>
        public async Task<WinnerResultDto?> DrawForGiftAsync(int giftId)
        {
            // 1. ��� �� �� ������� ������� �� ����� ���
            var orders = await _orderRepo.GetByGiftIdAsync(giftId);
            var confirmed = orders.Where(o => o.Status == Status.IsConfirmed).ToList();

            // 2. ��� ���� ������� ��� ����� ���� ����� ���
            var ticketsByUser = new Dictionary<int, int>(); // userId -> ticket count
            string giftName = string.Empty;

            foreach (var o in confirmed)
            {
                foreach (var oi in o.OrderItems.Where(i => i.GiftId == giftId))
                {
                    giftName = oi.Gift?.Name ?? giftName;
                    if (ticketsByUser.ContainsKey(o.UserId)) ticketsByUser[o.UserId] += oi.Quantity;
                    else ticketsByUser[o.UserId] = oi.Quantity;
                }
            }

            var totalTickets = ticketsByUser.Values.Sum();
            if (totalTickets == 0) return null;

            // 3. ����� ���� ��� ���� (���� �������)
            var rng = new Random();
            var pick = rng.Next(1, totalTickets + 1); // [1..totalTickets]
            var cumulative = 0;
            int winnerUserId = 0;
            foreach (var kv in ticketsByUser)
            {
                cumulative += kv.Value;
                if (pick <= cumulative)
                {
                    winnerUserId = kv.Key;
                    break;
                }
            }

            // 4. ��� ���� �����
            var user = await _userRepo.GetByIdAsync(winnerUserId);
            if (user == null)
            {
                _logger.LogWarning("Winner user {Id} not found in DB", winnerUserId);
                return null;
            }

            // 5. ����� ���� ����
            var winnerEntity = new Winner
            {
                GiftId = giftId,
                UserId = winnerUserId
            };

            var savedWinner = await _lotteryRepo.SaveWinnerAsync(winnerEntity);

            // 6. ���� DTO ������ ����� (Winners.csv)
            var result = new WinnerResultDto
            {
                GiftId = giftId,
                GiftName = giftName,
                WinnerUserId = winnerUserId,
                WinnerName = string.IsNullOrWhiteSpace(user.Name) ? user.Email : $"{user.Name} ",
                WinnerEmail = user.Email,
                TotalTickets = totalTickets,
                DrawDate = DateTime.UtcNow
            };            AppendWinnerReport(result);

            // ── Kafka event ──────────────────────────────────────────────
            try
            {
                var kafkaEvent = new LotteryDrawnEvent
                {
                    GiftId       = result.GiftId,
                    GiftName     = result.GiftName,
                    WinnerUserId = result.WinnerUserId,
                    WinnerName   = result.WinnerName,
                    WinnerEmail  = result.WinnerEmail ?? string.Empty,
                    TotalTickets = result.TotalTickets,
                    DrawnAt      = result.DrawDate
                };
                await _kafka.PublishAsync(_lotteryTopic, giftId.ToString(), kafkaEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish Kafka lottery event for gift #{GiftId}", giftId);
            }

            // ── Email ─────────────────────────────────────────────────────
            try
            {
                if (!string.IsNullOrWhiteSpace(result.WinnerEmail))
                {
                    var subject = $"🏆 מזל טוב! זכית במתנה: {result.GiftName}";
                    var body    = BuildWinnerEmail(result);
                    await _emailSender.SendEmailAsync(result.WinnerEmail, subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send winner notification email");
            }

            return result;
        }

        /// <summary>
        /// ���� ����� ��� ���� ������� ����� ������� (����� �� ����� ������),
        /// ���� �� �� ������ ����� ��� ������ (Revenue.csv).
        /// </summary>
        public async Task<IEnumerable<WinnerResultDto>> DrawAllAsync()
        {
            var results = new List<WinnerResultDto>();

            var gifts = await _giftRepo.GetAllAsync();

            foreach (var g in gifts)
            {
                var res = await DrawForGiftAsync(g.Id);
                if (res != null) results.Add(res);
            }

            var totalRevenue = await _lotteryRepo.GetTotalRevenueAsync();
            AppendRevenueReport(totalRevenue);

            return results;
        }

        // Append one line to Winners.csv (GiftId,GiftName,WinnerUserId,WinnerName,WinnerEmail,TotalTickets,DrawDate)
        private void AppendWinnerReport(WinnerResultDto result)
        {
            try
            {
                var file = Path.Combine(_reportsPath, "Winners.csv");
                var exists = File.Exists(file);
                using var sw = new StreamWriter(file, append: true);
                if (!exists)
                {
                    sw.WriteLine("GiftId,GiftName,WinnerUserId,WinnerName,WinnerEmail,TotalTickets,DrawDate");
                }

                // escape commas in fields
                string esc(string? s) => (s ?? string.Empty).Replace(",", " ");
                sw.WriteLine($"{result.GiftId},{esc(result.GiftName)},{result.WinnerUserId},{esc(result.WinnerName)},{esc(result.WinnerEmail)},{result.TotalTickets},{result.DrawDate.ToString("o", CultureInfo.InvariantCulture)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append winner report");
            }
        }        // Append or create Revenue.csv with timestamp and total revenue
        private void AppendRevenueReport(decimal totalRevenue)
        {
            try
            {
                var file = Path.Combine(_reportsPath, "Revenue.csv");
                var exists = File.Exists(file);
                using var sw = new StreamWriter(file, append: true);
                if (!exists)
                {
                    sw.WriteLine("GeneratedAt,TotalRevenue");
                }

                sw.WriteLine($"{DateTime.UtcNow:o},{totalRevenue}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append revenue report");
            }
        }

        /// <summary>
        /// Builds a detailed lottery winner notification email.
        /// </summary>
        private static string BuildWinnerEmail(WinnerResultDto result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"שלום {result.WinnerName},");
            sb.AppendLine();
            sb.AppendLine($"🏆 מזל טוב! זכית בהגרלת Chinese Auction!");
            sb.AppendLine();
            sb.AppendLine("פרטי הזכייה:");
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine($"  🎁 מתנה:          {result.GiftName}");
            sb.AppendLine($"  📅 תאריך הגרלה:   {result.DrawDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"  🎫 סה\"כ כרטיסים:  {result.TotalTickets}");
            sb.AppendLine($"  👤 פרטי הזוכה:    {result.WinnerName} ({result.WinnerEmail})");
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine("נציגנו יצרו איתך קשר בהקדם לתיאום מסירת הפרס.");
            sb.AppendLine();
            sb.AppendLine("בברכה,");
            sb.AppendLine("צוות Chinese Auction 🎉");
            return sb.ToString();
        }
    }
}