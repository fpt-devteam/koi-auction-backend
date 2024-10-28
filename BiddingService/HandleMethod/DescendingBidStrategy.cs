using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.IServices;
using AuctionService.Mappers;
using AuctionService.Services;

namespace AuctionService.HandleMethod
{
    public class DescendingBidStrategy : ABidStrategyService
    {
        private HighestBidLog? _winner = null;
        private decimal? _currentPrice;

        public DescendingBidStrategy()
        : base() // Truyền bidService đến constructor của lớp cha
        {

        }

        public override HighestBidLog? GetWinner()
        {
            return _winner;
        }

        public override bool IsBidValid(CreateBidLogDto bid, AuctionLotBidDto? auctionLotBidDto, decimal balance)
        {
            if (auctionLotBidDto == null || auctionLotBidDto.AuctionLotId != bid.AuctionLotId)
            {
                return false;
            }

            // Khởi tạo giá hiện tại nếu chưa có (bắt đầu với giá khởi điểm)
            _currentPrice ??= auctionLotBidDto.StartPrice;
            System.Console.WriteLine($"currentPrice = {_currentPrice}");

            // Kiểm tra nếu BidAmount đạt các tiêu chí và người mua có đủ số dư
            if (bid.BidAmount == _currentPrice && bid.BidAmount <= balance)
            {
                _winner = bid.ToHighestBidLogFromCreateBidLogDto(); // Cập nhật người thắng cuộc là người đầu tiên chấp nhận giá hiện tại
                return true;
            }

            // Giảm giá hiện tại nếu không có người chấp nhận
            _currentPrice -= CalculatePriceReduction(auctionLotBidDto.StartPrice, auctionLotBidDto.StepPercent);
            System.Console.WriteLine($"Updated currentPrice = {_currentPrice}");

            return false;
        }

        private decimal CalculatePriceReduction(decimal startPrice, int stepPercent)
        {
            return startPrice * stepPercent / 100;
        }

    }
}