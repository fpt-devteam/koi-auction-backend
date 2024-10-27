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
        private IServiceScope? _serviceScope;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private BidService? _bidService;
        public BidService? BidService => _bidService;

        public BidManagementService(IServiceScopeFactory serviceScopeFactory, IHubContext<BidHub> bidHub)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }



        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public void StartAuctionLot(AuctionLotBidDto auctionLotBidDto)
        {
            if (_serviceScope != null)
            {
                throw new Exception("There is an ongoing auction lot");
            }
            // Tạo scope mới cho phiên đấu giá
            _serviceScope = _serviceScopeFactory.CreateScope();
            _bidService = _serviceScope!.ServiceProvider.GetRequiredService<BidService>();
            _bidService.AuctionLotBidDto = auctionLotBidDto;
            // await _bidHub.Clients.Group(auctionLotBidDto.AuctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", auctionLotBidDto);
            //             System.Console.WriteLine($"StartAuctionLot {AuctionLotBidDto.AuctionLotId}");

            // Khởi tạo strategy đấu giá dựa trên auction lot method
            // var bidStrategy = GetBidStrategy(_serviceScope, auctionLotBidDto.AuctionMethodId);
            _bidService!.SetStrategy(auctionLotBidDto.AuctionMethodId); // Giả sử BidService có phương thức SetStrategy để cài đặt chiến lược
            // await _bidHub.Clients.Group(auctionLotBidDto.AuctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", auctionLotBidDto);
            Console.WriteLine($"StartAuctionLot {auctionLotBidDto.AuctionLotId}");
        }

        // public async Task<bool> IsBidValid(CreateBidLogDto createBidLogDto)
        // {
        //     if (_bidService == null)
        //     {
        //         throw new InvalidOperationException("Auction is not started or has ended.");
        //     }
        //     return await _bidService.IsBidValid(createBidLogDto, _auctionLotBid);
        // }

        // public CreateBidLogDto? GetWinner()
        // {
        //     return _bidService?.GetWinner();
        // }

        // // Kết thúc và dispose phiên đấu giá
        public void EndAuctionLot()
        {
            if (_serviceScope == null)
            {
                throw new Exception("There is no ongoing auction lot.");
            }
            System.Console.WriteLine("end");
            // await _bidHub.Clients.Group(auctionLotBidDto!.AuctionLotId.ToString()).SendAsync("ReceiveEndAuctionLot", auctionLotBidDto);
            var winner = _bidService!.GetWinner();
            System.Console.WriteLine($"winner = {winner!.BidderId}");
            // Dispose và reset các giá trị
            System.Console.WriteLine("end1");
            _serviceScope.Dispose();
            System.Console.WriteLine("end2");
            _serviceScope = null;
            System.Console.WriteLine("end3");
            _bidService = null;
            System.Console.WriteLine("End auction lot");
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
