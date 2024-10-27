using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Services
{
    // public class AuctionLotManagerService
    // {á
    //     private readonly IServiceScopeFactory _serviceScopeFactory;
    //     private IServiceScope? _scope; // Add this field to store the scope

    //     public AuctionLotManagerService(IServiceScopeFactory serviceScopeFactory)
    //     {
    //         _serviceScopeFactory = serviceScopeFactory;
    //     }

    //     public PlaceBidService StartAuctionLot(AuctionLotBidDto AuctionLotBidDto)
    //     {
    //         // Create a new scope and store it in the field
    //         _scope = _serviceScopeFactory.CreateScope();

    //         // Retrieve and set up the PlaceBidService
    //         var placeBidService = _scope.ServiceProvider.GetRequiredService<PlaceBidService>();
    //         placeBidService.SetUp(AuctionLotBidDto);

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

    public class BidManagementService
    {
        private IServiceScopeFactory _serviceScopeFactory;

        private IServiceScope? _serviceScope;

        private readonly IHubContext<BidHub> _bidHub;

        private BidService? _bidService;

        public BidService? BidService
        {
            get => _bidService;
        }

        public BidManagementService(IServiceScopeFactory serviceScopeFactory, IHubContext<BidHub> bidHub)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _bidHub = bidHub;
        }

        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public async Task StartAuctionLot(AuctionLotBidDto AuctionLotBidDto)
        {
            if (_serviceScope != null)
            {
                throw new Exception("There was an ONGOING auction lot");

            }
            _serviceScope = _serviceScopeFactory.CreateScope();
            _bidService = _serviceScope.ServiceProvider.GetRequiredService<BidService>();
            _bidService.AuctionLotBidDto = AuctionLotBidDto;
            await _bidHub.Clients.Group(AuctionLotBidDto.AuctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", AuctionLotBidDto);
            System.Console.WriteLine($"StartAuctionLot {AuctionLotBidDto.AuctionLotId}");
        }

        // Kết thúc và dispose phiên đấu giá
        public async Task EndAuctionLot()
        {
            if (_serviceScope == null)
            {
                throw new Exception("There is NO ongoing auction lot");
            }
            AuctionLotBidDto? AuctionLotBidDto = _bidService?.AuctionLotBidDto;
            await _bidHub.Clients.Group(AuctionLotBidDto!.AuctionLotId.ToString()).SendAsync("ReceiveEndAuctionLot", AuctionLotBidDto);
            _serviceScope.Dispose();
            _serviceScope = null;
        }

        public bool IsAuctionLotOngoing(int auctionLotId)
        {
            if (_serviceScope == null || _bidService == null || _bidService.AuctionLotBidDto == null)
            {
                return false; // Nếu _serviceScope hoặc _bidService hoặc AuctionLotBidDto là null
            }
            return _bidService.AuctionLotBidDto.AuctionLotId == auctionLotId;
        }

    }
}