using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.IServices;
using BiddingService.Mappers;
using BiddingService.Services;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Hubs
{
    public class BidHub : Hub
    {
        const string RECEIVE_EXCEPTION_MESSAGE = "ReceiveExceptionMessage";
        const string RECEIVE_CURRENT_BID = "ReceiveCurrentBid";

        private readonly BidManagementService _bidManagementService;
        private readonly BidService _bidService;
        // Khai báo chiến lược như một biến instance
        private readonly IDictionary<string, UserConnectionDto> _connections; // <connectionId, (uid, auctionLotId)>
        public BidHub(IDictionary<string, UserConnectionDto> connections, BidManagementService auctionLotManagerService, BidService bidService, IBidStrategy bidStrategy)
        {
            _connections = connections;
            _bidManagementService = auctionLotManagerService;
            _bidService = bidService;
        }
        // public async Task PlaceBid(CreateBidLogDto bid)
        // {
        //     System.Console.WriteLine("Place Bid");
        //     try
        //     {
        //         if (await _bidManagementService!.BidService!.IsBidValid(bid))
        //         {
        //             await Clients.Group(bid.AuctionLotId.ToString()).SendAsync("ReceivePlaceBid", bid);
        //             await _bidManagementService.BidService.AddBidLog(bid);
        //         }
        //         else
        //         {
        //             await Clients.Caller.SendAsync("ReceivePlaceBidErrorMessage", "Place Bid not valid");
        //         }

        //     }
        //     catch (Exception ex)
        //     {
        //         System.Console.WriteLine(ex.ToString());
        //     }


        // }
        public async Task EndAuctionLot()
        {
            try
            {
                _bidManagementService.EndAuctionLot();
                await Clients.Caller.SendAsync("ReceiveEndAuctionLot", "End auction lot");
            }
            catch
            {
                await Clients.Caller.SendAsync("ReceiveEndAuctionLotErrorMessage", "There is no ongoing auction lot");

            }

        }

        public void StartAuctionLot(AuctionLotBidDto auctionLotBidDto)
        {
            try
            {
                int auctionLotId = auctionLotBidDto.AuctionLotId;
                _bidManagementService.StartAuctionLot(auctionLotBidDto);

                // Gửi message đến tất cả client trong group
                Clients.Caller.SendAsync("ReceiveStartAuctionLot", auctionLotBidDto);
                Clients.Group(auctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", auctionLotBidDto);
            }
            catch (Exception e)
            {
                Clients.Caller.SendAsync("ReceiveStartAuctionLotErrorMessage", "There is an ongoing auction lot");
                Clients.Caller.SendAsync("ReceiveExceptionMessage", e.Message);
            }
        }

        public async Task JoinAuctionLot(UserConnectionDto userConnection)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.AuctionLotId.ToString());
                _connections[Context.ConnectionId] = userConnection;
                await SendCurrentBid(userConnection.AuctionLotId);
                Console.WriteLine($"User {(userConnection.UserId == null ? "Guest" : userConnection.UserId)} joined auction lot {userConnection.AuctionLotId}");
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync(RECEIVE_EXCEPTION_MESSAGE, e.Message);
            }
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnectionDto? connection))
            {
                _connections.Remove(Context.ConnectionId);
                System.Console.WriteLine($"User {connection.UserId} left auction lot {connection.AuctionLotId}");
            }
            return base.OnDisconnectedAsync(exception);
        }
        public void IsAuctionLotOngoing(int auctionLotId)
        {
            bool isExist = _bidManagementService.IsAuctionLotOngoing(auctionLotId);
            string mess = (isExist) ? $"Auction lot {auctionLotId} is ongoing" : $"Auction lot {auctionLotId} is NOT ongoing";
            Clients.Caller.SendAsync("ReceiveExceptionMessage", mess);
        }
        public async Task PlaceBid(CreateBidLogDto bid)
        {
            try
            {
                // await _bidManagementService.StartAuctionLot(new AuctionLotBidDto { AuctionLotId = bid.AuctionLotId });
                // await _bidManagementService.PlaceBid(bid)
                await Clients.All.SendAsync(RECEIVE_CURRENT_BID, bid.BidAmount);
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync(RECEIVE_EXCEPTION_MESSAGE, e.Message);
            }
        }
        private async Task SendCurrentBid(int auctionLotId)
        {
            try
            {
                // var currentBid = await _bidManagementService.GetCurrentBid(auctionLotId);
                int currentBid = 100;
                await Clients.Caller.SendAsync(RECEIVE_CURRENT_BID, null);
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync(RECEIVE_EXCEPTION_MESSAGE, e.Message);
            }
        }
    }
}