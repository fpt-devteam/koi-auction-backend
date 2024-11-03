using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.IServices
{
    public interface IAuctionLotService
    {
        public Task ScheduleAuctionLotAsync(int auctionLotId, DateTime startTime);
        public Task StartAuctionLotAsync(int auctionLotId);

    }
}