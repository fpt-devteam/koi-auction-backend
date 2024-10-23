using BiddingService.Dto.AuctionLot;

namespace BiddingService.Services
{
    // public class AuctionLotManagerService
    // {
    //     private readonly IServiceScopeFactory _serviceScopeFactory;
    //     private IServiceScope? _scope; // Add this field to store the scope

    //     public AuctionLotManagerService(IServiceScopeFactory serviceScopeFactory)
    //     {
    //         _serviceScopeFactory = serviceScopeFactory;
    //     }

    //     public PlaceBidService StartAuctionLot(AuctionLotDto auctionLotDto)
    //     {
    //         // Create a new scope and store it in the field
    //         _scope = _serviceScopeFactory.CreateScope();

    //         // Retrieve and set up the PlaceBidService
    //         var placeBidService = _scope.ServiceProvider.GetRequiredService<PlaceBidService>();
    //         placeBidService.SetUp(auctionLotDto);

    //         return placeBidService;
    //     }

    //     public void EndAuction()
    //     {
    //         // Dispose of the scope to end the auction
    //         _scope?.Dispose();
    //     }
    // }

    //
    //
    //

    public class AuctionLotManagerService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Dictionary<int, IServiceScope> _activeAuctions;

        public AuctionLotManagerService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _activeAuctions = new Dictionary<int, IServiceScope>();
        }

        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public void StartAuction(int auctionLotId, AuctionLotDto auctionLotDto)
        {
            if (_activeAuctions.ContainsKey(auctionLotId))
            {
                System.Console.WriteLine("Auction already started for this lot.");
                return;
            }

            var scope = _serviceScopeFactory.CreateScope();
            var placeBidService = scope.ServiceProvider.GetRequiredService<PlaceBidService>();

            placeBidService.SetUp(auctionLotDto);

            _activeAuctions[auctionLotId] = scope;
        }

        // Lấy PlaceBidService theo auctionLotId
        public PlaceBidService? GetPlaceBidService(int auctionLotId)
        {
            if (_activeAuctions.TryGetValue(auctionLotId, out var scope))
            {
                return scope.ServiceProvider.GetRequiredService<PlaceBidService>();
            }
            return null;
        }

        // Kết thúc và dispose phiên đấu giá
        public void EndAuction(int auctionLotId)
        {
            if (_activeAuctions.TryGetValue(auctionLotId, out var scope))
            {
                scope.Dispose(); // Giải phóng scope
                _activeAuctions.Remove(auctionLotId); // Xóa phiên đấu giá khỏi danh sách
            }
        }
    }
}


