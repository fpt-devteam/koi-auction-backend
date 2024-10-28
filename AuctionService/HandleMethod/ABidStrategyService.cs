using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.IServices;


namespace AuctionService.HandleMethod
{
    public abstract class ABidStrategyService : IBidStrategy
    {
        public abstract bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance);
        public abstract HighestBidLog? GetWinner();
    }
}