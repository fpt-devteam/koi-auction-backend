using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.Auction;
using AuctionService.Dto.AuctionLot;
using AuctionService.Models;

namespace AuctionService.IServices
{
    public interface IAuctionLotService
    {
        public Task ScheduleAuctionLot(int auctionLotId, DateTime startTime);

        public Task ScheduleEndAuctionLot(int auctionLotId, DateTime endTime);

        public Task UpdateEndTimeAuctionLot(int auctionLotId, DateTime newEndTime);
        Task<AuctionLot> DeleteAsync(int id);
        Task<bool> DeleteListAsync(List<int> ids);
        Task<AuctionLot> CreateAsync(CreateAuctionLotDto auctionLot);
        Task<List<AuctionLot>> CreateListAsync(List<CreateAuctionLotDto> auctionLots);
        //test only
        public Task StartAuctionLot(int auctionLotId);

    }
}