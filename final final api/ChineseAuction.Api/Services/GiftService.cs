using AutoMapper;
using ChineseAuction.Api.Dtos;
using ChineseAuction.Api.Models;
using ChineseAuction.Api.Repositories;

namespace ChineseAuction.Api.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _repo;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        private readonly TimeSpan _ttl;

        // Cache key constants
        private const string KeyAll = "gifts:all";
        private const string KeyAdmin = "gifts:admin";
        private const string KeyByPriceAsc = "gifts:price:asc";
        private const string KeyByPriceDesc = "gifts:price:desc";
        private const string KeyByCategory = "gifts:category";
        private static string KeyById(int id) => $"gifts:{id}";

        // All keys that must be cleared when any gift changes
        private static readonly string[] AllGiftKeys =
        [
            KeyAll, KeyAdmin, KeyByPriceAsc, KeyByPriceDesc, KeyByCategory
        ];

        public GiftService(IGiftRepository repo, IMapper mapper, ICacheService cache, IConfiguration config)
        {
            _repo = repo;
            _mapper = mapper;
            _cache = cache;
            var ttlMinutes = config.GetValue<int>("Redis:CacheTtlMinutes:Gifts", 10);
            _ttl = TimeSpan.FromMinutes(ttlMinutes);
        }

        public async Task<IEnumerable<GiftDetailDto>> GetAllForBuyersAsync()
        {
            var cached = await _cache.GetAsync<IEnumerable<GiftDetailDto>>(KeyAll);
            if (cached is not null) return cached;

            var gifts = await _repo.GetAllAsync();
            var result = _mapper.Map<IEnumerable<GiftDetailDto>>(gifts);
            await _cache.SetAsync(KeyAll, result, _ttl);
            return result;
        }

        public async Task<IEnumerable<GiftDetailDto>> GetAllSortedByPriceAsync(bool ascending)
        {
            var key = ascending ? KeyByPriceAsc : KeyByPriceDesc;
            var cached = await _cache.GetAsync<IEnumerable<GiftDetailDto>>(key);
            if (cached is not null) return cached;

            var gifts = await _repo.GetAllSortedByPriceAsync(ascending);
            var result = _mapper.Map<IEnumerable<GiftDetailDto>>(gifts);
            await _cache.SetAsync(key, result, _ttl);
            return result;
        }

        public async Task<IEnumerable<GiftDetailDto>> GetAllSortedByCategoryAsync()
        {
            var cached = await _cache.GetAsync<IEnumerable<GiftDetailDto>>(KeyByCategory);
            if (cached is not null) return cached;

            var gifts = await _repo.GetAllSortedByCategoryAsync();
            var result = _mapper.Map<IEnumerable<GiftDetailDto>>(gifts);
            await _cache.SetAsync(KeyByCategory, result, _ttl);
            return result;
        }

        public async Task<IEnumerable<GiftAdminDto>> GetAllForAdminAsync()
        {
            var cached = await _cache.GetAsync<IEnumerable<GiftAdminDto>>(KeyAdmin);
            if (cached is not null) return cached;

            var gifts = await _repo.GetAllAsync();
            var result = _mapper.Map<IEnumerable<GiftAdminDto>>(gifts);
            await _cache.SetAsync(KeyAdmin, result, _ttl);
            return result;
        }

        public async Task<GiftDetailDto?> GetByIdAsync(int id)
        {
            var key = KeyById(id);
            var cached = await _cache.GetAsync<GiftDetailDto>(key);
            if (cached is not null) return cached;

            var gift = await _repo.GetByIdAsync(id);
            var result = _mapper.Map<GiftDetailDto>(gift);
            if (result is not null)
                await _cache.SetAsync(key, result, _ttl);
            return result;
        }

        // Search is not cached — dynamic query with many combinations
        public async Task<IEnumerable<GiftDto>> SearchAsync(string? name, string? donor, int? minPurchasers)
        {
            var gifts = await _repo.SearchGiftsInternalAsync(name, donor, minPurchasers);
            return _mapper.Map<IEnumerable<GiftDto>>(gifts);
        }

        public async Task<int> AddToDonorAsync(int donorId, GiftCreateUpdateDto dto, string? imagePath)
        {
            if (!await _repo.DonorExistsAsync(donorId))
                throw new KeyNotFoundException("הדונור המבוקש לא נמצא במערכת");

            var gift = _mapper.Map<Gift>(dto);
            gift.DonorId = donorId;
            gift.ImageUrl = imagePath;
            gift.CategoryId = (int)dto.CategoryId;
            gift.Category = null;

            var id = await _repo.CreateAsync(gift);
            await InvalidateAllGiftCacheAsync();
            return id;
        }

        public async Task<bool> UpdateAsync(int id, GiftCreateUpdateDto dto, string? imagePath)
        {
            var existing = await _repo.GetByIdTrackedAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);

            if (dto.CategoryId.HasValue)
            {
                existing.CategoryId = dto.CategoryId.Value;
                existing.Category = null;
            }

            if (!string.IsNullOrEmpty(imagePath))
                existing.ImageUrl = imagePath;

            await _repo.SaveChangesAsync();
            await InvalidateAllGiftCacheAsync(id);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (deleted)
                await InvalidateAllGiftCacheAsync(id);
            return deleted;
        }

        private async Task InvalidateAllGiftCacheAsync(int? specificId = null)
        {
            var keys = specificId.HasValue
                ? [.. AllGiftKeys, KeyById(specificId.Value)]
                : AllGiftKeys;
            await _cache.RemoveAsync(keys);
        }
    }
}
