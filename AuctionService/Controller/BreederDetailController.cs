using System.Text.Json;
using AuctionService.Dto.BreederDetail;
using Microsoft.Extensions.Caching.Memory;

namespace AuctionService.Controller
{
    public class BreederDetailController(HttpClient httpClient, IMemoryCache cache, IHttpContextAccessor http)
    {
        private readonly HttpClient? _httpClient = httpClient;
        private readonly IMemoryCache? _cache = cache;
        private readonly IHttpContextAccessor _http = http;

        public async Task<BreederDetailDto?> GetBreederByIdAsync(int breederId)
        {
            var httpContext = _http.HttpContext;
            if (!_cache!.TryGetValue(breederId, out BreederDetailDto? breeder))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:3000/user-service/manage/breeder/profile/{breederId}");

                // Sao chép tất cả các header từ yêu cầu gốc
                foreach (var header in httpContext!.Request.Headers)
                {
                    if (!request.Headers.Contains(header.Key))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
                var response = await _httpClient!.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    breeder = JsonSerializer.Deserialize<BreederDetailDto>(content);

                    // Lưu vào cache với thời gian sống 10 phút
                    _cache!.Set(breederId, breeder, TimeSpan.FromMinutes(10));
                }
            }

            return breeder;
        }
    }
}
