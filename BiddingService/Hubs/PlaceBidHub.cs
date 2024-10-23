using System.Collections.Concurrent;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.Services;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Hubs
{
    public class PlaceBidHub : Hub
    {
        private readonly IDictionary<string, UserConnectionDto> _connections;
        private readonly PlaceBidService _placeBidService;

        public PlaceBidHub(IDictionary<string, UserConnectionDto> connections, PlaceBidService placeBidService)
        {
            _connections = connections;
            _placeBidService = placeBidService;
        }


        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnectionDto? userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.AuctionLotId.ToString()).SendAsync("ReceiveMessage", $"{userConnection.BidderId} has left");
                SendUsersConnected(userConnection.AuctionLotId.ToString());
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinAuctionLot(UserConnectionDto userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.AuctionLotId.ToString());

            _connections[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.AuctionLotId.ToString()).SendAsync("BroadcastMessage", $"{userConnection.BidderId} has joined {userConnection.AuctionLotId}");

            await SendUsersConnected(userConnection.AuctionLotId.ToString());
        }


        public Task SendUsersConnected(string room)
        {
            var users = _connections.Values
                .Where(c => c.AuctionLotId.ToString() == room)
                .Select(c => c.BidderId);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }


        // Nhận message từ client và check tính hợp lệ
        public async Task SendMessagePlaceBid(CreateBidLogDto bidMessage)
        {
            System.Console.WriteLine("hihiii");
            // Gọi logic kiểm tra tính hợp lệ
            var isValid = _placeBidService.ValidateBid(bidMessage);

            if (isValid)
            {
                System.Console.WriteLine("do cho nay");
                System.Console.WriteLine($"auction lot id {bidMessage.AuctionLotId}");
                System.Console.WriteLine($"bidder {bidMessage.BidderId}");

                //gửi message đến tất cả các client trong room
                await Clients.Group(bidMessage.AuctionLotId.ToString())
                    .SendAsync("ReceivePlaceBid", bidMessage);

                // //lưu bid vào db
                // _placeBidService.AddBidLog(bidMessage);
            }
            else
            {
                //Trả về lỗi cho client đã gửi message
                await Clients.Caller.SendAsync("BidFailed", "Your bid is invalid.");
            }
        }


        // Phương thức để gửi message cho tất cả các client
        public async Task BroadcastMessage(UserConnectionDto userConnection)
        {
            await Clients.All.SendAsync("ReceiveMessage", userConnection);
        }
    }

}