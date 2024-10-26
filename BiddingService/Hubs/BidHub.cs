using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.Services;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Hubs
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
        private readonly IDictionary<string, UserConnectionDto> _connections; // <connectionId, (uid, auctionLotId)>
        public BidHub(IDictionary<string, UserConnectionDto> connections, BidManagementService auctionLotManagerService)
        {
            _connections = connections;
            _bidManagementService = auctionLotManagerService;
        }
        // public void StartAuctionLot(AuctionLotDto auctionLotDto)
        // {
        //     try
        //     {
        //         int auctionLotId = auctionLotDto.AuctionLotId;
        //         if (_bidManagementService.StartAuctionLot(auctionLotDto))
        //         {
        //             // Gửi message đến tất cả client trong group
        //             Clients.Caller.SendAsync("ReceiveStartAuctionLot", auctionLotDto);
        //             Clients.Group(auctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", auctionLotDto);
        //         }
        //         else
        //         {
        //             // Gửi message đến client gửi request
        //             Clients.Caller.SendAsync("ReceiveStartAuctionLotErrorMessage", "There is an ongoing auction lot");
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Clients.Caller.SendAsync("ReceiveExceptionMessage", e.Message);
        //     }
        // }
        // public void EndAuctionLot()
        // {
        //     AuctionLotDto? auctionLotDto = _bidManagementService.EndAuctionLot();
        //     if (auctionLotDto != null)
        //     {
        //         Clients.Caller.SendAsync("ReceiveEndAuctionLot", auctionLotDto);
        //         Clients.Group(auctionLotDto.AuctionLotId.ToString()).SendAsync("ReceiveEndAuctionLot", auctionLotDto);
        //     }
        //     else
        //     {
        //         Clients.Caller.SendAsync("ReceiveEndAuctionLotErrorMessage", "There is no ongoing auction lot");
        //     }
        // }
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