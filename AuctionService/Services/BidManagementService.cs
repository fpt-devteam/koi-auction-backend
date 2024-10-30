using System.Collections.Concurrent;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.Dto.SoldLot;
using AuctionService.Dto.UserConnection;
using AuctionService.HandleMethod;
using AuctionService.Hubs;
using AuctionService.IServices;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.Services
{
    public class BidManagementService
    {
        private IServiceScope? _serviceScope;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private BidService? _bidService;
        public BidService? BidService => _bidService;
        private readonly IDictionary<string, UserConnectionDto> _connections;
        private readonly IHubContext<BidHub> _bidHub;

        public BidManagementService(IServiceScopeFactory serviceScopeFactory, IHubContext<BidHub> bidHub, IDictionary<string, UserConnectionDto> connections)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _serviceScope = null;
            _bidService = null;
            _bidHub = bidHub;
            _connections = connections;
        }

        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public async Task StartAuctionLotAsync(AuctionLotBidDto auctionLotBidDto)
        {
            if (_serviceScope != null)
            {
                _serviceScope.Dispose();
                _serviceScope = null;
                _bidService = null;
            }
            // Tạo scope mới cho phiên đấu giá
            _serviceScope = _serviceScopeFactory.CreateScope();
            _bidService = _serviceScope!.ServiceProvider.GetRequiredService<BidService>();
            _bidService.AuctionLotBidDto = auctionLotBidDto;

            // Khởi tạo strategy đấu giá dựa trên auction lot method
            _bidService.SetStrategy(auctionLotBidDto);
            await _bidHub.Clients.Group(auctionLotBidDto.AuctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", auctionLotBidDto);
            Console.WriteLine($"BidManagement Receive auction lot bid dto {auctionLotBidDto.AuctionLotId}");
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

        // public void StartExtendPhase()
        // {
        //     if (_bidService == null)
        //     {
        //         throw new Exception("There is no ongoing auction lot.");
        //     }
        //     _bidService.StartExtendPhase();
        // }
        public async Task EndAuctionLotAsync()
        {
            if (_serviceScope == null)
            {
                throw new Exception("There is no ongoing auction lot.");
            }
            var winner = _bidService!.GetWinner();
            if (winner == null) System.Console.WriteLine("No winner");
            else
                System.Console.WriteLine($"winner = {winner!.BidderId}");
            await _bidHub.Clients.Group(_bidService!.AuctionLotBidDto!.AuctionLotId.ToString()).SendAsync("ReceiveEndAuctionLot", winner);
            System.Console.WriteLine("Bidding Management Service: End auction lot");
            // send message to winner to connection has userId = winner.BidderId

            if (winner != null)
            {
                string connectionId = _connections.FirstOrDefault(x => x.Value.UserId == winner.BidderId).Key;
                await _bidHub.Clients.User(connectionId).SendAsync("ReceiveWinner", winner);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var soldLotService = scope.ServiceProvider.GetRequiredService<ISoldLotService>();
                    await soldLotService.CreateSoldLot(new CreateSoldLotDto
                    {
                        SoldLotId = _bidService.AuctionLotBidDto.AuctionLotId,
                        WinnerId = winner.BidderId,
                        FinalPrice = winner.BidAmount
                    });
                }
                await _bidHub.Clients.All.SendAsync("ReceiveWinner", winner);
            }

            //call httpClient to payment

            // Dispose và reset các giá trị
            _serviceScope.Dispose();
            _serviceScope = null;
            _bidService = null;

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
