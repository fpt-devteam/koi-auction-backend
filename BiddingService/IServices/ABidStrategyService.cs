using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.Dto.SoldLot;
using AuctionService.Models;
using AuctionService.Services;

namespace AuctionService.IServices
{
    public abstract class ABidStrategyService : IBidStrategy
    {
        public abstract bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance);
        public abstract HighestBidLog? GetWinner();
    }
}