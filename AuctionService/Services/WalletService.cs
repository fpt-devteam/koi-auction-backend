using System.Text;
using System.Text.Json;
using AuctionService.Dto.Wallet;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace AuctionService.Services
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
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _walletCache = new();
        }

        // Phương thức lấy token sử dụng username và password từ appsettings
        private async Task<string?> LoginAsync()
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
                System.Console.WriteLine("login success");
                return tokenResponse?.AccessToken; // Giả sử TokenResponse có thuộc tính Token
            }
            System.Console.WriteLine("login fail");
            return null;
        }

        // Phương thức lấy số dư ví và lưu vào cache nếu chưa có
        public async Task<WalletDto?> GetBalanceByIdAsync(int id)
        {
            System.Console.WriteLine($"call api wallet {id}");
            System.Console.WriteLine("call api wallet");
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

            // await LoginAsync();
            // Gọi PaymentService để lấy thông tin ví
            var response = await _httpClient.GetAsync($"http://localhost:3000/payment-service/internal/get-wallet-balance/{id}");
            // var response = await _httpClient.GetAsync($"https://67035c76bd7c8c1ccd412a4e.mockapi.io/api/wallet/{id}");
            System.Console.WriteLine("call api wallet 2");
            System.Console.WriteLine(response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                System.Console.WriteLine("call api wallet 3");
                var content = await response.Content.ReadAsStringAsync();
                wallet = JsonSerializer.Deserialize<WalletDto>(content);
                _walletCache[id] = wallet!; // Lưu vào cache tạm thời
            }

            return wallet;
        }
    }
}
