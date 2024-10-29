using System.Collections.Concurrent;
using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.HandleMethod;
using BiddingService.Hubs;
using BiddingService.IServices;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Services
{
    public class BidManagementService
    {
        const string RECEIVE_START_AUCTION_LOT = "ReceiveStartAuctionLot";
        const string RECEIVE_END_AUCTION_LOT = "ReceiveEndAuctionLot";
        private IServiceScopeFactory _serviceScopeFactory;

        private IServiceScope? _serviceScope;
        // private readonly IServiceScopeFactory _serviceScopeFactory;
        private BidService? _bidService;
        public BidService? BidService => _bidService;

        public IHubContext<BidHub> _bidHub { get; }

        public BidManagementService(IServiceScopeFactory serviceScopeFactory, IHubContext<BidHub> bidHub)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _bidHub = bidHub;
        }



        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public async Task StartAuctionLot(AuctionLotBidDto auctionLotBidDto)
        {
            if (_serviceScope != null)
            {
                throw new Exception("There is an ongoing auction lot");
            }
            // Tạo scope mới cho phiên đấu giá
            _serviceScope = _serviceScopeFactory.CreateScope();
            _bidService = _serviceScope.ServiceProvider.GetRequiredService<BidService>();
            _bidService.AuctionLotBidDto = auctionLotBidDto;
            await _bidHub.Clients.All.SendAsync(RECEIVE_START_AUCTION_LOT);
            System.Console.WriteLine($"Start Auction Lot {auctionLotBidDto.AuctionLotId}");
        }

        // Kết thúc và dispose phiên đấu giá
        public async Task EndAuctionLot()
        {
            if (_serviceScope == null)
            {
                throw new Exception("There is no ongoing auction lot.");
            }
            await _bidHub.Clients.All.SendAsync(RECEIVE_END_AUCTION_LOT);
            _serviceScope.Dispose();
            System.Console.WriteLine("end2");
            _serviceScope = null;
            System.Console.WriteLine($"End Auction Lot");
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
