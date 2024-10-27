using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.SoldLot;
using BiddingService.Models;
using BiddingService.Services;

namespace BiddingService.IServices
{
    public abstract class ABidStrategyService : IBidStrategy
    {
        public abstract bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance);
        public abstract HighestBidLog? GetWinner();
    }
}