using System.Collections.Concurrent;
using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.Dto.SoldLot;
using BiddingService.IServices;
using BiddingService.Mappers;
using BiddingService.Models;
using BiddingService.Services;

namespace BiddingService.HandleMethod
{
    public class FixedPriceBidStrategy : ABidStrategyService
    {
        private readonly List<CreateBidLogDto> _bids; // list de chon winner

        public FixedPriceBidStrategy()
        : base() // Truyền bidService đến constructor của lớp cha
        {
            _bids = new();
        }

        public override HighestBidLog? GetWinner()
        {
            if (_bids == null || _bids.Count == 0)
                return null;

            Random random = new Random();
            int randomIndex = random.Next(_bids.Count);
            return _bids[randomIndex].ToHighestBidLogFromCreateBidLogDto();
        }


        public override bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance)
        {
            //System.Console.WriteLine("hihi");

            System.Console.WriteLine($"bid {auctionLotBidDto!.AuctionLotId} = {bid.AuctionLotId} start price = {auctionLotBidDto.StartPrice}");
            //kiểm tra AuctionLotStaus
            if (auctionLotBidDto != null && auctionLotBidDto!.AuctionLotId == bid.AuctionLotId
                    //&& _cacheService.GetBalance(bid.BidderId) <= bid.BidAmount
                    && bid.BidAmount == auctionLotBidDto.StartPrice)
            {
                System.Console.WriteLine($"Fixed: 54 {balance}");

                if (bid.BidAmount <= balance)
                {
                    _bids.Add(bid);
                    //PrintAllBids();
                    return true;
                }

            }
            return false;
        }
        //test
        // public void PrintAllBids()
        // {
        //     foreach (var bid in _bids)
        //     {
        //         Console.WriteLine($"Bid ID: {bid.BidderId}, Amount: {bid.BidAmount}");
        //     }
        // }

    }
}