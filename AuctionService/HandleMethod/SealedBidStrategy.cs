using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Services;

namespace AuctionService.HandleMethod
{
    public class SealedBidStrategy : ABidStrategyService
    {
        private readonly List<CreateBidLogDto> _bids;
        private decimal? _highestBid;
        public SealedBidStrategy()
        : base() // Truyền bidService đến constructor của lớp cha
        {
            _bids = new();
        }
        public override HighestBidLog? GetWinner()
        {
            if (_bids == null || _bids.Count == 0)
                return null;
            else if (_bids.Count == 1)
                return _bids[0].ToHighestBidLogFromCreateBidLogDto();
            else
            {
                Random random = new Random();
                int randomIndex = random.Next(_bids.Count);
                return _bids[randomIndex].ToHighestBidLogFromCreateBidLogDto();
            }

        }

        public override bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance)
        {
            // Kiểm tra điều kiện cơ bản về AuctionLot và số dư trước
            if (auctionLotBidDto == null || auctionLotBidDto.AuctionLotId != bid.AuctionLotId || bid.BidAmount > balance)
            {
                return false;
            }

            // Cập nhật _highestBid và kiểm tra xem bid có hợp lệ không
            if (_highestBid == null || bid.BidAmount > _highestBid)
            {
                _highestBid = bid.BidAmount;
                _bids.Clear(); // Xóa các bid có cùng giá trị với _highestBid trước đó
            }
            else if (bid.BidAmount == _highestBid)
            {
                _bids.Add(bid);
            }

            return true;
        }

    }
}