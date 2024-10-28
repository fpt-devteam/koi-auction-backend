using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.Dto.UserConnection;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Services;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.Hubs
{
    public class BidHub : Hub
    {
        //     public override Task OnDisconnectedAsync(Exception? exception)
        //     {
        //         if (_connections.TryGetValue(Context.ConnectionId, out UserConnectionDto? userConnection))
        //         {
        //             _connections.Remove(Context.ConnectionId);
        //             Clients.Group(userConnection.AuctionLotId.ToString()).SendAsync("ReceiveMessage", $"{userConnection.UserId} has left");
        //             SendUsersConnected(userConnection.AuctionLotId.ToString());
        //         }

        //         return base.OnDisconnectedAsync(exception);
        //     }

        //     // public void SendAuctionLotDto(AuctionLotDto auctionLotDto)
        //     // {
        //     //     _placeBidService!.SetUp(auctionLotDto);
        //     //     System.Console.WriteLine($"start price = {auctionLotDto.StartPrice}");
        //     // }

        //     public void UpdateUserBalanced(int userId, int balance)
        //     {
        //         // _cacheService.SetBalance(userId, balance, TimeSpan.FromDays(1));
        //     }
        //     public Task SendUsersConnected(string room)
        //     {
        //         var users = _connections.Values
        //             .Where(c => c.AuctionLotId.ToString() == room)
        //             .Select(c => c.UserId);

        //         return Clients.Group(room).SendAsync("UsersInRoom", users);
        //     }

        //     // Phương thức để gửi message cho tất cả các client
        //     public async Task BroadcastMessage(UserConnectionDto userConnection)
        //     {
        //         await Clients.All.SendAsync("ReceiveMessage", userConnection);
        //     }
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
        public async Task PlaceBid(CreateBidLogDto bid)
        {
            System.Console.WriteLine("Place Bid");
            try
            {
                if (await _bidManagementService!.BidService!.IsBidValid(bid))
                {
                    await Clients.Group(bid.AuctionLotId.ToString()).SendAsync("ReceivePlaceBid", bid);
                    await _bidManagementService.BidService.AddBidLog(bid);
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceivePlaceBidErrorMessage", "Place Bid not valid");
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }


        }
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
                Console.WriteLine($"User {(userConnection.UserId == null ? "Guest" : userConnection.UserId)} joined auction lot {userConnection.AuctionLotId}");
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("ReceiveExceptionMessage", e.Message);
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
            Clients.Caller.SendAsync("ReceiveIsAuctionLotOngoingMessage", mess);
        }
    }
}