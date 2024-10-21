using System.Collections.Concurrent;
using BiddingService.Dto.BidLog;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Hubs
{
    public class PlaceBidHub : Hub
    {

        // Phương thức để gửi message cho tất cả các client
        public async Task BroadcastMessage(CreateBidLogDto dto)
        {
            await Clients.All.SendAsync("ReceiveMessage", dto);
        }
    }

}