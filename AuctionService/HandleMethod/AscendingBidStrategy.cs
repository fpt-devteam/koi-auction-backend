using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Services;

namespace AuctionService.HandleMethod
{
    public class AscendingBidStrategy : ABidStrategyService
    {
        private HighestBidLog? _winner = null;
        private decimal? _standardPrice;
        private decimal? _stepPrice;
        public AscendingBidStrategy()
        : base()
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

            // Khởi tạo _standardPrice và _stepPrice nếu chưa có
            _standardPrice ??= auctionLotBidDto.StartPrice;
            System.Console.WriteLine($"standerPrice = {_standardPrice}");
            _stepPrice ??= CalculateStepPrice(auctionLotBidDto.StartPrice, auctionLotBidDto.StepPercent);
            System.Console.WriteLine($"stepPrice = {_stepPrice}");
            // Kiểm tra nếu BidAmount đạt các tiêu chí
            if (bid.BidAmount >= _standardPrice && bid.BidAmount <= balance)
            {
                _standardPrice = bid.BidAmount + _stepPrice;
                System.Console.WriteLine($"hehe standerPrice = {_standardPrice}");
                _winner = bid.ToHighestBidLogFromCreateBidLogDto();
                return true;
            }

            return false;
        }

        private decimal CalculateStepPrice(decimal? startPrice, int? stepPercent)
        {
            if (startPrice == null || stepPercent == null)
                throw new ArgumentNullException("Start Price And Step Percent must be not null");
            return (decimal)(startPrice * stepPercent / 100);
        }

    }
}