using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.SoldLot;
using BiddingService.Models;

namespace BiddingService.IServices
{
    public interface IBidStrategy
    {
        bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance);
        HighestBidLog? GetWinner();
    }
}