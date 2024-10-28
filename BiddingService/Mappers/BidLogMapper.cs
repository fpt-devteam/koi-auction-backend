using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.BidLog;
using AuctionService.Models;

namespace AuctionService.Mappers
{
    public static class BidLogMapper
    {
        public static BidLogDto ToBidLogDtoFromBidLog(this BidLog bidLog)
        {
            return new BidLogDto
            {
                BidLogId = bidLog.BidLogId,
                BidderId = bidLog.BidderId,
                AuctionLotId = bidLog.AuctionLotId,
                BidAmount = bidLog.BidAmount,
                BidTime = bidLog.BidTime
            };
        }

        public static BidLog ToBidLogFromCreateBidLogDto(this CreateBidLogDto createBidLogDto)
        {
            return new BidLog
            {
                BidderId = createBidLogDto.BidderId,
                AuctionLotId = createBidLogDto.AuctionLotId,
                BidAmount = createBidLogDto.BidAmount
            };
        }
    }
}