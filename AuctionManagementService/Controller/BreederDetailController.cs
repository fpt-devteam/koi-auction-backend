using System.Text.Json;
using AuctionManagementService.Dto.BreederDetail;
using Microsoft.Extensions.Caching.Memory;

namespace AuctionManagementService.Controller
{
    public class BreederDetailController(HttpClient httpClient, IMemoryCache cache)
    {
        private readonly HttpClient? _httpClient = httpClient;
        private readonly IMemoryCache? _cache = cache;

        public async Task<BreederDetailDto?> GetBreederByIdAsync(int breederId)
        {
            if (!_cache!.TryGetValue(breederId, out BreederDetailDto? breeder))
            {
                var response = await _httpClient!.GetAsync($"https://67035c76bd7c8c1ccd412a4e.mockapi.io/api/profiles/{breederId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    breeder = JsonSerializer.Deserialize<BreederDetailDto>(content);
                    _cache!.Set(breederId, breeder, TimeSpan.FromMinutes(10));
                }
            }

            return breeder;
        }
    }
}