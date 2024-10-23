using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.IServices
{
    public interface IAuctionLotService
    {
        public Task ScheduleAuctionLot(int auctionLotId, DateTime startTime, TimeOnly duration);

        public Task ScheduleEndAuctionLot(int auctionLotId, DateTime endTime);

        public Task UpdateEndTimeAuctionLot(int auctionLotId, DateTime newEndTime);

    }
}