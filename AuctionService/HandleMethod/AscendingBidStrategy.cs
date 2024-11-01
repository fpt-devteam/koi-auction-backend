using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.Helper;
using AuctionService.Hubs;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Services;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.HandleMethod
{
    public class AscendingBidStrategy : ABidStrategyService
    {
        private HighestBidLog? _winner = null;
        private Timer? _timer = null;

        private decimal? _standardPrice;
        private decimal? _stepPrice;
        private IHubContext<BidHub> _bidHub;
        private AuctionLotBidDto? _auctionLotBidDto;

        private const int DESCREASE_INTERVAL = 1;

        private const int EXTENDED_TIME = 20;

        public event Func<int, Task>? CountdownFinished;

        public AscendingBidStrategy(IHubContext<BidHub> bidHub)
        : base()
        {
            _bidHub = bidHub;
        }

        public void SetUp(AuctionLotBidDto auctionLotBidDto)
        {
            System.Console.WriteLine($"SetUp called");
            _auctionLotBidDto = auctionLotBidDto;
            _standardPrice = auctionLotBidDto.StartPrice;
            _stepPrice = auctionLotBidDto.StepPercent * auctionLotBidDto.StartPrice / 100;
            _timer = new Timer(ExtendedTimeDescrease, null, auctionLotBidDto.RemainingTime - TimeSpan.FromSeconds(EXTENDED_TIME), TimeSpan.FromSeconds(1));

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
            // _standardPrice ??= auctionLotBidDto.StartPrice;
            System.Console.WriteLine($"standerPrice = {_standardPrice}");
            // _stepPrice ??= CalculateStepPrice(auctionLotBidDto.StartPrice, auctionLotBidDto.StepPercent);
            // System.Console.WriteLine($"stepPrice = {_stepPrice}");
            // Kiểm tra nếu BidAmount đạt các tiêu chí
            if (bid.BidAmount >= _standardPrice && bid.BidAmount <= balance)
            {
                _standardPrice = bid.BidAmount + _stepPrice;
                System.Console.WriteLine($"hehe standerPrice = {_standardPrice}");
                _winner = bid.ToHighestBidLogFromCreateBidLogDto();

                // if RemainingTime < 20s
                if (_auctionLotBidDto!.RemainingTime < TimeSpan.FromSeconds(EXTENDED_TIME))
                {
                    _auctionLotBidDto!.RemainingTime = TimeSpan.FromSeconds(EXTENDED_TIME);
                }
                return true;
            }

            return false;
        }

        private void ExtendedTimeDescrease(object? state)
        {
            if (_auctionLotBidDto!.RemainingTime > TimeSpan.FromSeconds(EXTENDED_TIME))
            {
                _auctionLotBidDto!.RemainingTime = TimeSpan.FromSeconds(EXTENDED_TIME);
            }
            _auctionLotBidDto.PredictEndTime = DateTime.Now.Add(_auctionLotBidDto.RemainingTime);
            _bidHub.Clients.All.SendAsync(WsMess.ReceivePredictEndTime, _auctionLotBidDto.PredictEndTime);
            // _bidHub.Clients.Group(_auctionLotBidDto!.AuctionLotId.ToString()).SendAsync("ReceivePredictEndTime", _auctionLotBidDto.PredictEndTime);
            System.Console.WriteLine($"Remaining Time = {_auctionLotBidDto!.RemainingTime}");
            if (_auctionLotBidDto!.RemainingTime <= TimeSpan.Zero)
            {
                System.Console.WriteLine($"Zero Zero = {_auctionLotBidDto!.RemainingTime}");
                _timer!.Dispose();
                CountdownFinished?.Invoke(_auctionLotBidDto!.AuctionLotId);
            }
            else
            {
                _auctionLotBidDto!.RemainingTime = _auctionLotBidDto!.RemainingTime.Subtract(TimeSpan.FromSeconds(1));
            }
        }


        // private decimal CalculateStepPrice(decimal? startPrice, int? stepPercent)
        // {
        //     if (startPrice == null || stepPercent == null)
        //         throw new ArgumentNullException("Start Price And Step Percent must be not null");
        //     return (decimal)(startPrice * stepPercent / 100);
        // }

    }
}