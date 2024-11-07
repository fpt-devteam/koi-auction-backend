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
        private IServiceScope? _serviceScope;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private BidService? _bidService;
        public BidService? BidService => _bidService;
        private readonly IDictionary<string, UserConnectionDto> _connections;
        private readonly IHubContext<BidHub> _bidHub;
        private readonly HttpClient _httpClient;

        public BidManagementService(HttpClient httpClient, IServiceScopeFactory serviceScopeFactory, IHubContext<BidHub> bidHub, IDictionary<string, UserConnectionDto> connections)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _serviceScope = null;
            _bidService = null;
            _bidHub = bidHub;
            _connections = connections;
            _httpClient = httpClient;
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
            await _bidHub.Clients.All.SendAsync(WsMess.ReceiveStartAuctionLot);
            await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchAuctionLot);
            await _bidHub.Clients.All.SendAsync(WsMess.ReceivePredictEndTime, BidService!.GetPredictEndTime());
            if (auctionLotBidDto.AuctionMethodId == (int)Enums.BidMethodType.AscendingBid)
                await _bidHub.Clients.All.SendAsync(WsMess.ReceiveWinner, BidService!.GetWinner());
            if (auctionLotBidDto.AuctionMethodId == (int)Enums.BidMethodType.DescendingBid)
                await _bidHub.Clients.All.SendAsync(WsMess.ReceivePriceDesc, BidService.GetPriceDesc());
            Console.WriteLine($"BidManagement Receive auction lot bid dto {auctionLotBidDto.AuctionLotId}");
        }

        public async Task EndAuctionLotAsync()
        {
            if (_serviceScope == null)
            {
                throw new Exception("There is no ongoing auction lot.");
            }
            var winner = _bidService!.GetWinner();

            await _bidHub.Clients.All.SendAsync(WsMess.ReceiveWinner, winner);
            await _bidHub.Clients.All.SendAsync(WsMess.ReceiveEndAuctionLot);
            await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchAuctionLot);
            await _bidHub.Clients.All.SendAsync(WsMess.ReceiveFetchBidLog);

            //test only
            if (winner == null) System.Console.WriteLine("No winner");
            else
                System.Console.WriteLine($"winner = {winner!.BidderId}");
            System.Console.WriteLine("Bidding Management Service: End auction lot");

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    // var lot = await unitOfWork.Lots.GetLotByIdAsync(_bidService.AuctionLotBidDto!.AuctionLotId);
                    // lot.LotStatusId = winner == null ? (int)Enums.LotStatus.UnSold : (int)Enums.LotStatus.ToShip;
                    //move to the bot to save change one time
                    if (winner != null)
                    {
                        System.Console.WriteLine($"Winner : {winner.BidderId}");
                        var soldLot = new Models.SoldLot
                        {
                            WinnerId = winner.BidderId,
                            FinalPrice = winner.BidAmount,
                            SoldLotId = _bidService.AuctionLotBidDto!.AuctionLotId
                        };
                        await unitOfWork.SoldLot.CreateSoldLot(soldLot);

                        await _bidService.PaymentAsync(new PaymentDto
                        {
                            UserId = winner.BidderId,
                            Amount = winner.BidAmount
                        });
                        //send message to winner
                        var connection = _connections.FirstOrDefault(x => x.Value.UserId == winner.BidderId);
                        if (connection.Value != null)
                        {
                            await _bidHub.Clients.Client(connection.Key).SendAsync(WsMess.ReceiveSuccessPayment, new PaymentDto
                            {
                                UserId = winner.BidderId,
                                Amount = winner.BidAmount
                            });
                        }
                    }
                    var lot = await unitOfWork.Lots.GetLotByIdAsync(_bidService.AuctionLotBidDto!.AuctionLotId);
                    lot.LotStatusId = winner == null ? (int)Enums.LotStatus.UnSold : (int)Enums.LotStatus.ToShip;
                    await unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.Message);
            }


            // Dispose và reset các giá trị
            _serviceScope.Dispose();
            _serviceScope = null;
            _bidService = null;

        }

        // public bool IsAuctionLotOngoing(int auctionLotId)
        // {
        //     if (_serviceScope == null || _bidService == null || _bidService.AuctionLotBidDto == null)
        //     {
        //         return false; // Nếu _serviceScope hoặc _bidService hoặc AuctionLotBidDto là null
        //     }
        //     return _bidService.AuctionLotBidDto.AuctionLotId == auctionLotId;
        // }
    }
}
