using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.UserConnection;
using BiddingService.Services;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Hubs
{
    public class AuctionLotHub : Hub
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
        private readonly AuctionLotService _auctionLotService;
        private readonly IDictionary<string, UserConnectionDto> _connections; // <connectionId, (uid, auctionLotId)>
        public AuctionLotHub(IDictionary<string, UserConnectionDto> connections, AuctionLotService auctionLotManagerService)
        {
            _connections = connections;
            _auctionLotService = auctionLotManagerService;
        }
        public void StartAuctionLot(AuctionLotDto auctionLotDto)
        {
            try
            {
                int auctionLotId = auctionLotDto.AuctionLotId;
                if (_auctionLotService.StartAuctionLot(auctionLotDto))
                {
                    // Gửi message đến tất cả client trong group
                    Clients.Caller.SendAsync("ReceiveStartAuctionLot", auctionLotDto);
                    Clients.Group(auctionLotId.ToString()).SendAsync("ReceiveStartAuctionLot", auctionLotDto);
                }
                else
                {
                    // Gửi message đến client gửi request
                    Clients.Caller.SendAsync("ReceiveStartAuctionLotErrorMessage", "There is an ongoing auction lot");
                }
            }
            catch (Exception e)
            {
                Clients.Caller.SendAsync("ReceiveExceptionMessage", e.Message);
            }
        }
        public void EndAuctionLot()
        {
            AuctionLotDto? auctionLotDto = _auctionLotService.EndAuctionLot();
            if (auctionLotDto != null)
            {
                Clients.Caller.SendAsync("ReceiveEndAuctionLot", auctionLotDto);
                Clients.Group(auctionLotDto.AuctionLotId.ToString()).SendAsync("ReceiveEndAuctionLot", auctionLotDto);
            }
            else
            {
                Clients.Caller.SendAsync("ReceiveEndAuctionLotErrorMessage", "There is no ongoing auction lot");
            }
        }
        public async Task JoinAuctionLot(UserConnectionDto userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.AuctionLotId.ToString());
            _connections[Context.ConnectionId] = userConnection;
            System.Console.WriteLine($"User {userConnection.UserId} joined auction lot {userConnection.AuctionLotId}");
        }
        public void IsAuctionLotOngoing(int auctionLotId)
        {
            bool isExist = _auctionLotService.IsAuctionLotOngoing(auctionLotId);
            string mess = (isExist) ? $"Auction lot {auctionLotId} is ongoing" : $"Auction lot {auctionLotId} is NOT ongoing";
            Clients.Caller.SendAsync("ReceiveIsAuctionLotOngoingMessage", mess);
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

        public async Task PlaceBid(decimal bidAmount)
        {
            try
            {
                //get userid and auctionId from connectionId
                var connectionId = Context.ConnectionId;
                if (!_connections.TryGetValue(connectionId, out UserConnectionDto? userConnection))
                {
                    int userId = userConnection!.UserId;
                    int auctionLotId = userConnection!.AuctionLotId;
                    CreateBidLogDto createBidLogDto = new CreateBidLogDto
                    {
                        BidderId = userId,
                        AuctionLotId = auctionLotId,
                        BidAmount = bidAmount
                    };
                    if (_auctionLotService.IsAuctionLotOngoing(auctionLotId))
                    {
                        if (_auctionLotService!.AuctionLotBidService!.IsBidValid(createBidLogDto))
                        {
                            await Clients.Group(auctionLotId.ToString()).SendAsync("ReceivePlaceBid", createBidLogDto);
                            await _auctionLotService!.AuctionLotBidService!.AddBidLog(createBidLogDto);
                        }
                        else
                        {
                            await Clients.Caller.SendAsync("ReceivePlaceBidError", "Your bid is invalid.");
                        }
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("ReceivePlaceBidError", e.Message);
            }



            //print out the bid amount, auction lot id and bidder id
            // System.Console.WriteLine($"Bid amount: {bidMessage.BidAmount}");
            // System.Console.WriteLine($"Auction lot id: {bidMessage.AuctionLotId}");
            // System.Console.WriteLine($"Bidder id: {bidMessage.BidderId}");

            // var isValid = _auctionLotManagerService.ValidateBid(auctionLotId, bidMessage);
            // if (isValid)
            // {
            //     await Clients.Group(bidMessage.AuctionLotId.ToString()).SendAsync("ReceivePlaceBid", bidMessage);
            // }
            // else
            // {
            //     await Clients.Caller.SendAsync("ReceivePlaceBid", "Your bid is invalid.");
            // }
        }

        // public async Task IsAuctionLotOngoing(int auctionLotId)
        // {
        //     bool isExist = _auctionLotService.IsAuctionLotOngoing(auctionLotId);
        //     string mess = (isExist) ? $"Auction lot {auctionLotId} is ongoing" : $"Auction lot {auctionLotId} is NOT ongoing";
        //     await Clients.Caller.SendAsync("ReceiveMessage", mess);
        // }


    }
}