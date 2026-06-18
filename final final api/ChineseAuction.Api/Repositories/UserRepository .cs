//using ChineseAuction.Api.Data;
//using ChineseAuction.Api.Models;
//using Microsoft.EntityFrameworkCore;

//namespace ChineseAuction.Api.Repositories
//{
//    public class UserRepository : IUserRepository
//    {
//        private readonly AppDbContext _context;

//        public UserRepository(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<User>> GetAllAsync()
//        {
//            return await _context.Users.ToListAsync();
//        }

//        public async Task<User?> GetByIdAsync(int id)
//        {
//            return await _context.Users
//               // .Include(u => u.Orders)
//                .FirstOrDefaultAsync(u => u.Id == id);
//        }

//        public async Task<User?> GetByEmailAsync(string email)
//        {
//            return await _context.Users
//                .FirstOrDefaultAsync(u => u.Email == email);
//        }

//        // יצירת משתמש חדש
//        public async Task<User> CreateAsync(User user)
//        {
//            user.CreatedAt = DateTime.UtcNow;
//            user.UpdatedAt = DateTime.UtcNow;

//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();
//            return user;
//        }

//        // עדכון משתמש קיים
//        public async Task<User?> UpdateAsync(User user)
//        {
//            var existing = await _context.Users.FindAsync(user.Id);
//            if (existing == null) return null;

//            _context.Entry(existing).CurrentValues.SetValues(user);
//            existing.UpdatedAt = DateTime.UtcNow;

//            await _context.SaveChangesAsync();
//            return existing;
//        }

//        // מחיקת משתמש לפי מזהה
//        public async Task<bool> DeleteAsync(int id)
//        {
//            var user = await _context.Users.FindAsync(id);
//            if (user == null) return false;

//            _context.Users.Remove(user);
//            await _context.SaveChangesAsync();
//            return true;
//        }

//        // בדיקת קיום משתמש לפי מזהה
//        public async Task<bool> ExistsAsync(int id)
//        {
//            return await _context.Users.AnyAsync(u => u.Id == id);
//        }


//        // בדיקת קיום משתמש לפי אימייל
//        public async Task<bool> EmailExistsAsync(string email)
//        {
//            return await _context.Users.AnyAsync(u => u.Email == email);
//        }
//    }
//}



using ChineseAuction.Api.Data;
using ChineseAuction.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChineseAuction.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        //constructor
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        //קבלת כל המשתמשים
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        //חיפוש משתמש לפי id
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        //חיפוש משתמש לפי מייל
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        //יצירת משתמש חדש
        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        //בדוק אם משתמש קיים לפי id
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        //בדוק אם משתמש קיים לפי מייל 
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}
