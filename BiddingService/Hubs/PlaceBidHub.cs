using System.Collections;
using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.IServices;
using BiddingService.Services;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Hubs
{
    public class PlaceBidHub : Hub
    {
        // private readonly IDictionary<string, UserConnectionDto> _connections;
        //     private PlaceBidService? _placeBidService;
        //     // private readonly ICacheService _cacheService;
        //     private readonly AuctionLotManagerService _auctionManagerService;

        //     public PlaceBidHub(IDictionary<string, UserConnectionDto> connections, AuctionLotManagerService auctionManagerService)
        //     {
        //         _connections = connections;
        //         // _cacheService = cacheService;
        //         _auctionManagerService = auctionManagerService;
        //     }

        //     // Khi phiên đấu giá bắt đầu
        //     public void StartAuctionLot(AuctionLotDto auctionLotDto)
        //     {
        //         System.Console.WriteLine("hehehehe");
        //         _placeBidService = _auctionManagerService.StartAuctionLot(auctionLotDto);
        //     }

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

        // public async Task JoinAuctionLot(UserConnectionDto userConnection)
        // {
        //     await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.AuctionLotId.ToString());

        //     _connections[Context.ConnectionId] = userConnection;

        //     await Clients.Group(userConnection.AuctionLotId.ToString()).SendAsync("BroadcastMessage", $"{userConnection.UserId} has joined {userConnection.AuctionLotId}");

        //     await SendUsersConnected(userConnection.AuctionLotId.ToString());
        // }

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


        //     // Nhận message từ client và check tính hợp lệ
        //     public async Task SendMessagePlaceBid(CreateBidLogDto bidMessage)
        //     {

        //         if (_placeBidService == null)
        //         {
        //             System.Console.WriteLine("PlaceBidService is null, check initialization.");
        //             await Clients.Caller.SendAsync("BidFailed", "PlaceBidService not initialized.");
        //             return;
        //         }
        //         System.Console.WriteLine("hihiii");
        //         // if (!_cacheService.IsUserBalanceInCache(bidMessage.BidderId))
        //         // {
        //         //     // call api từ service dieuvi
        //         // }
        //         // Gọi logic kiểm tra tính hợp lệ
        //         // System.Console.WriteLine(bidMessage.BidAmount);
        //         // System.Console.WriteLine(bidMessage.AuctionLotId);
        //         // System.Console.WriteLine(bidMessage.BidderId);
        //         // return;

        //         var isValid = _placeBidService!.ValidateBid(bidMessage);

        //         if (isValid)
        //         {
        //             System.Console.WriteLine("do cho nay");
        //             System.Console.WriteLine($"auction lot id {bidMessage.AuctionLotId}");
        //             System.Console.WriteLine($"bidder {bidMessage.BidderId}");

        //             //gửi message đến tất cả các client trong room
        //             await Clients.Group(bidMessage.AuctionLotId.ToString())
        //                 .SendAsync("ReceivePlaceBid", bidMessage);

        //             // //lưu bid vào db
        //             // _placeBidService.AddBidLog(bidMessage);
        //         }
        //         else
        //         {
        //             //Trả về lỗi cho client đã gửi message
        //             await Clients.Caller.SendAsync("BidFailed", "Your bid is invalid.");
        //         }
        //     }


        //     // Phương thức để gửi message cho tất cả các client
        //     public async Task BroadcastMessage(UserConnectionDto userConnection)
        //     {
        //         await Clients.All.SendAsync("ReceiveMessage", userConnection);
        //     }
        private readonly IDictionary<string, UserConnectionDto> _connections;
        private readonly AuctionLotManagerService _auctionManagerService;
        private PlaceBidService? _placeBidService; // Biến instance của lớp PlaceBidHub

        public PlaceBidHub(IDictionary<string, UserConnectionDto> connections, AuctionLotManagerService auctionManagerService)
        {
            _connections = connections;
            _auctionManagerService = auctionManagerService;
        }

        public async Task JoinAuctionLot(UserConnectionDto userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.AuctionLotId.ToString());

            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.AuctionLotId.ToString()).SendAsync("BroadcastMessage", $"{userConnection.UserId} has joined {userConnection.AuctionLotId}");

            // await SendUsersConnected(userConnection.AuctionLotId.ToString());
        }
        // Khi phiên đấu giá bắt đầu
        public void StartAuctionLot(int auctionLotId, AuctionLotDto auctionLotDto)
        {
            // Gọi AuctionManagerService để khởi tạo phiên đấu giá
            _auctionManagerService.StartAuction(auctionLotId, auctionLotDto);
        }

        public async Task SendMessagePlaceBid(CreateBidLogDto bidMessage)
        {
            var placeBidService = _auctionManagerService.GetPlaceBidService(bidMessage.AuctionLotId);
            if (placeBidService == null)
            {
                System.Console.WriteLine("PlaceBidService is null, check initialization.");
                await Clients.Caller.SendAsync("BidFailed", "No active auction for this lot.");
                return;
            }

            var isValid = placeBidService.ValidateBid(bidMessage);
            if (isValid)
            {
                await Clients.Group(bidMessage.AuctionLotId.ToString()).SendAsync("ReceivePlaceBid", bidMessage);
            }
            else
            {
                await Clients.Caller.SendAsync("BidFailed", "Your bid is invalid.");
            }
        }


    }
}