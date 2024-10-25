using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;

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

    public class AuctionLotService
    {
        private IServiceScopeFactory _serviceScopeFactory;
        //
        private IServiceScope? _serviceScope;

        private AuctionLotBidService? _auctionLotBidService;

        public AuctionLotBidService? AuctionLotBidService
        {
            get => _auctionLotBidService;
        }

        public AuctionLotService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public bool StartAuctionLot(AuctionLotDto auctionLotDto)
        {
            try
            {
                if (_serviceScope != null)
                {
                    return false;
                }
                _serviceScope = _serviceScopeFactory.CreateScope();
                _auctionLotBidService = _serviceScope.ServiceProvider.GetRequiredService<AuctionLotBidService>();
                _auctionLotBidService.AuctionLotDto = auctionLotDto;
                return true;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return false;
            }

        }

        // Kết thúc và dispose phiên đấu giá
        public AuctionLotDto? EndAuctionLot()
        {
            if (_serviceScope == null)
            {
                return null;
            }
            AuctionLotDto? auctionLotDto = _auctionLotBidService?.AuctionLotDto;
            _serviceScope.Dispose();
            _serviceScope = null;
            return auctionLotDto;
        }

        public bool IsAuctionLotOngoing(int auctionLotId)
        {
            if (_serviceScope == null || _auctionLotBidService == null || _auctionLotBidService.AuctionLotDto == null)
            {
                return false; // Nếu _serviceScope hoặc _auctionLotBidService hoặc AuctionLotDto là null
            }
            return _auctionLotBidService.AuctionLotDto.AuctionLotId == auctionLotId;
        }

    }
}