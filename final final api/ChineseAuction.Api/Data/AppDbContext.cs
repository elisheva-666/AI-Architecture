using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChineseAuction.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // הגדרת הטבלאות ב-Database
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Donor> Donors => Set<Donor>();
        public DbSet<Gift> Gifts => Set<Gift>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Winner> Winners => Set<Winner>();

        // public DbSet<Manager> Managers { get; set; } //  לצורך התחברות הנהלה

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. הגדרת קשר 1:1 בין מתנה לזוכה
            modelBuilder.Entity<Winner>()
                .HasOne(w => w.Gift)
                .WithOne(g => g.Winner)
                .HasForeignKey<Winner>(w => w.GiftId);

            // 2. הגנה על נתונים (Restrict): מניעת מחיקת מתנה אם קיימת לה הזמנה
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Gift)
                .WithMany(g => g.OrderItems)
                .HasForeignKey(oi => oi.GiftId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. הגדרת דיוק לשדה כספי (Decimal)
            modelBuilder.Entity<Gift>()
                .Property(g => g.TicketPrice)
                .HasPrecision(18, 2);

            // 4. נתוני התחלה (Seed Data) למנהל מערכת
            // כפי שצוין באפיון: המנהלים כבר קיימים מראש ב-DB
            //modelBuilder.Entity<Manager>().HasData(
            //    new Manager
            //    {
            //        Id = 1,
            //        Username = "admin",
            //        // הערה: במציאות יש לשים כאן Hash של סיסמה, לא טקסט פשוט
            //        PasswordHash = "Admin123!",
            //        Role = "manager"
            //    }
            //);
        }
    }
}