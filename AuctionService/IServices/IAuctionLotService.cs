using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.IServices
{
    public interface IAuctionLotService
    {
        // public void ScheduleAuctionLot(int auctionLotId, DateTime startTime);

        public Task StartAuctionLot(int auctionLotId, DateTime startTime);

        public Task EndAuctionLot(int auctionLotId, DateTime endTime);

    }
}