using System.Collections.Concurrent;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.Dto.SoldLot;
using AuctionService.Dto.UserConnection;
using AuctionService.Dto.Wallet;
using AuctionService.HandleMethod;
using AuctionService.Helper;
using AuctionService.Hubs;
using AuctionService.IRepository;
using AuctionService.IServices;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.Services
{
    public class BidManagementService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private ConcurrentDictionary<int, BidService> _bidServices;
        public ConcurrentDictionary<int, BidService> BidServices
        {
            get => _bidServices;
            private set => _bidServices = value;
        }
        public ConcurrentDictionary<int, IServiceScope> _auctionLotScopes;
        private readonly IDictionary<string, UserConnectionDto> _connections;
        private readonly IHubContext<BidHub> _bidHub;
        private readonly HttpClient _httpClient;

        public BidManagementService(HttpClient httpClient, IServiceScopeFactory serviceScopeFactory, IHubContext<BidHub> bidHub, IDictionary<string, UserConnectionDto> connections)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _auctionLotScopes = new ConcurrentDictionary<int, IServiceScope>();
            _bidServices = new ConcurrentDictionary<int, BidService>();
            _bidHub = bidHub;
            _connections = connections;
            _httpClient = httpClient;
        }

        // Bắt đầu phiên đấu giá và khởi tạo PlaceBidService trong scope riêng
        public async Task StartAuctionLotAsync(AuctionLotBidDto auctionLotBidDto)
        {
            Console.WriteLine($"BidManagement: Receive auction lot bid dto {auctionLotBidDto.AuctionLotId}");
            // if (_serviceScope != null)
            // {
            //     _serviceScope.Dispose();
            //     _serviceScope = null;
            //     _bidService = null;
            // }
            // Tạo scope mới cho phiên đấu giá
            _auctionLotScopes[auctionLotBidDto.AuctionLotId] = _serviceScopeFactory.CreateScope();
            _bidServices[auctionLotBidDto.AuctionLotId] = _auctionLotScopes[auctionLotBidDto.AuctionLotId].ServiceProvider.GetRequiredService<BidService>();

            BidService curBidService = _bidServices[auctionLotBidDto.AuctionLotId];

            curBidService.AuctionLotBidDto = auctionLotBidDto;
            curBidService.SetStrategy(auctionLotBidDto);

            string auctionLotId = auctionLotBidDto.AuctionLotId.ToString();

            // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveStartAuctionLot);
            // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchAuctionLot);
            // await _bidHub.Clients.All.SendAsync(WsMess.ReceivePredictEndTime, curBidService.GetPredictEndTime());
            await _bidHub.Clients.Group(auctionLotId).SendAsync(WsMess.ReceiveStartAuctionLot);
            await _bidHub.Clients.Group(auctionLotId).SendAsync(WsMess.ReceiveFetchAuctionLot);
            await _bidHub.Clients.Group(auctionLotId).SendAsync(WsMess.ReceivePredictEndTime, curBidService.GetPredictEndTime());

            if (auctionLotBidDto.AuctionMethodId == (int)Enums.BidMethodType.AscendingBid)
            {
                // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveWinner, curBidService.GetWinner());
                await _bidHub.Clients.Group(auctionLotId).SendAsync(WsMess.ReceiveWinner, curBidService.GetWinner());
            }
            if (auctionLotBidDto.AuctionMethodId == (int)Enums.BidMethodType.DescendingBid)
            {
                // await _bidHub.Clients.All.SendAsync(WsMess.ReceivePriceDesc, curBidService.GetPriceDesc());
                await _bidHub.Clients.Group(auctionLotId).SendAsync(WsMess.ReceivePriceDesc, curBidService.GetPriceDesc());
            }
        }

        public async Task EndAuctionLotAsync(int auctionLotId)
        {
            System.Console.WriteLine($"Bidding Management Service: End auction lot {auctionLotId}");
            if (!_auctionLotScopes.ContainsKey(auctionLotId) || !_bidServices.ContainsKey(auctionLotId))
            {
                System.Console.WriteLine("Bidding Management Service: Auction lot is not started yet");
                return;
            }
            BidService curBidService = _bidServices[auctionLotId];
            var winner = curBidService.GetWinner();

            //send loading
            // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveLoading);
            await _bidHub.Clients.Group(auctionLotId.ToString()).SendAsync(WsMess.ReceiveLoading);

            if (winner == null) System.Console.WriteLine($"Winner for auction lot {auctionLotId}: No winner");
            else
                System.Console.WriteLine($"Winner for auction lot {auctionLotId}: {winner!.BidderId}");

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var lot = await unitOfWork.Lots.GetLotByIdAsync(curBidService.AuctionLotBidDto!.AuctionLotId);
                    lot.LotStatusId = winner == null ? (int)Enums.LotStatus.UnSold : (int)Enums.LotStatus.ToShip;
                    if (winner != null)
                    {
                        var soldLot = new Models.SoldLot
                        {
                            WinnerId = winner.BidderId,
                            FinalPrice = winner.BidAmount,
                            SoldLotId = curBidService.AuctionLotBidDto!.AuctionLotId
                        };
                        await unitOfWork.SoldLot.CreateSoldLot(soldLot);
                    }
                    await unitOfWork.SaveChangesAsync();
                    // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveWinner, winner);
                    // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchAuctionLot);
                    // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchBidLog);
                    // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchWinnerPrice);
                    // await _bidHub.Clients.All.SendAsync(WsMess.ReceiveEndAuctionLot);
                    await _bidHub.Clients.Group(auctionLotId.ToString()).SendAsync(WsMess.ReceiveWinner, winner);
                    await _bidHub.Clients.Group(auctionLotId.ToString()).SendAsync(WsMess.ReceiveFetchAuctionLot);
                    await _bidHub.Clients.Group(auctionLotId.ToString()).SendAsync(WsMess.ReceiveFetchBidLog);
                    await _bidHub.Clients.Group(auctionLotId.ToString()).SendAsync(WsMess.ReceiveFetchWinnerPrice);
                    await _bidHub.Clients.Group(auctionLotId.ToString()).SendAsync(WsMess.ReceiveEndAuctionLot);
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.Message);
            }
            // Dispose và reset các giá trị
            // _serviceScope.Dispose();
            // _serviceScope = null;
            // _bidService = null;
            _auctionLotScopes[auctionLotId].Dispose();
            _auctionLotScopes.TryRemove(auctionLotId, out _);
            _bidServices.TryRemove(auctionLotId, out _);
        }
    }
}
