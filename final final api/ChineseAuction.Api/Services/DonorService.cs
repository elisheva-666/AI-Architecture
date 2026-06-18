
using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;

namespace ChineseAuction.Api.Services
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepository _donorRepo;
        private readonly IGiftRepository _giftRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<DonorService> _logger;

        public DonorService(IDonorRepository donorRepo, IGiftRepository giftRepo, IMapper mapper, ILogger<DonorService> logger)
        {
            _donorRepo = donorRepo;
            _giftRepo = giftRepo;
            _mapper = mapper;
            _logger = logger;
        }

        // מקבל Entities מה-Repo וממיר אותם ל-DonorDto עבור ה-Controller
        public async Task<IEnumerable<DonorDto>> GetAllAsync()
        {
            var donors = await _donorRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<DonorDto>>(donors);
        }

        // מקבל תורם עם מתנות וממיר ל-DonorDetailDto (המיפוי מתבצע אוטומטית ע"י AutoMapper)
        public async Task<DonorDetailDto?> GetByIdAsync(int id)
        {
            var donor = await _donorRepo.GetByIdAsync(id);
            if (donor == null) return null;
            return _mapper.Map<DonorDetailDto>(donor);
        }

        //מחפש תורמים לפי מייל
        public async Task<IEnumerable<DonorDto>> GetByEmailAsync(string email)
        {
            var donors = await _donorRepo.GetByEmailAsync(email);
            return _mapper.Map<IEnumerable<DonorDto>>(donors);
        }

        //מחפש תורמים לפי מייל
        public async Task<IEnumerable<DonorDto>> GetByNameAsync(string name)
        {
            var donors = await _donorRepo.GetByNameAsync(name);
            return _mapper.Map<IEnumerable<DonorDto>>(donors);
        }

        // לוגיקה עסקית ליצירת תורם: בדיקת כפילות והמרה מ-DTO ל-Entity
        public async Task<int> CreateAsync(DonorCreateDto dto)
        {
            try
            {
                // מיפוי ה-DTO לישות (ה-Mapper הופך את NewGifts ל-Gifts אוטומטית)
                var donor = _mapper.Map<Donor>(dto);

                // שמירה בבסיס הנתונים
                // ה-Repository מחזיר את ה-ID (int), וזה מה שאנחנו מחזירים ל-Controller
                int newId = await _donorRepo.CreateAsync(donor);

                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת תורם");
                throw;
            }
        }

        // עדכון תורם: טעינת הקיים, מיפוי השינויים ושמירה
        public async Task<bool> UpdateAsync(int id, DonorUpdateDto dto)
        {

            var existingDonor = await _donorRepo.GetByIdAsync(id);

            if (existingDonor == null) return false;
            _mapper.Map(dto, existingDonor);
            await _donorRepo.UpdateAsync(existingDonor);

            return true;
        }

        // מחיקה והחזרת הרשימה המעודכנת
        public async Task<IEnumerable<DonorDto>> DeleteAsync(int id)
        {
            var deleted = await _donorRepo.DeleteAsync(id);
            if (!deleted) _logger.LogWarning($"ניסיון מחיקה נכשל עבור תורם מספר {id}");

            return await GetAllAsync();
        }
    }
}