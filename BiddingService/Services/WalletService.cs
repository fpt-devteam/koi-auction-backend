using System.Text;
using System.Text.Json;
using BiddingService.Dto.Wallet;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace BiddingService.Services
{
    public class WalletService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<int, WalletDto> _walletCache;
        private readonly IConfiguration _configuration;

        public WalletService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _walletCache = new();
        }

        // Phương thức lấy token sử dụng username và password từ appsettings
        private async Task<string?> GetTokenAsync()
        {
            var username = _configuration["BiddingService:Username"];
            var password = _configuration["BiddingService:Password"];
            var tokenUrl = _configuration["BiddingService:TokenUrl"];

            var credentials = new { Username = username, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(tokenUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseContent); // accesstoken
                return tokenResponse?.AccessToken; // Giả sử TokenResponse có thuộc tính Token
            }

            return null;
        }

        // Phương thức lấy số dư ví và lưu vào cache nếu chưa có
        public async Task<WalletDto?> GetBalanceByIdAsync(int id)
        {
            if (_walletCache.TryGetValue(id, out var wallet))
            {
                return wallet;
            }

            // var token = await GetTokenAsync();
            // if (token == null)
            // {
            //     throw new Exception("Unable to retrieve token");
            // }

            // Thêm token vào header
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Gọi PaymentService để lấy thông tin ví
            //var response = await _httpClient.GetAsync($"http://localhost:3000/payment-service/manage/wallet/{id}");
            var response = await _httpClient.GetAsync($"https://67035c76bd7c8c1ccd412a4e.mockapi.io/api/wallet/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                wallet = JsonSerializer.Deserialize<WalletDto>(content);
                _walletCache[id] = wallet!; // Lưu vào cache tạm thời
            }

            return wallet;
        }
    }
}
