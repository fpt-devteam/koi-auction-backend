using System.Collections.Concurrent;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.HandleMethod;
using AuctionService.Hubs;
using AuctionService.IServices;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.Services
{
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