using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.Hubs;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Models;
using AuctionService.Services;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.HandleMethod
{
    public class DescendingBidStrategy : ABidStrategyService
    {
        private HighestBidLog? _winner = null;
        private readonly Timer _timer;
        private IHubContext<BidHub> _bidHub;
        private AuctionLotBidDto? _auctionLotBidDto;
        private decimal? _currentPrice;
        private decimal? _stepPrice;
        public event Func<int, Task>? CountdownFinished;

        private const int _decreaseInterval = 10;

        public DescendingBidStrategy(IHubContext<BidHub> bidHub)
        : base() // Truyền bidService đến constructor của lớp cha
        {
            // Khởi tạo giá hiện tại nếu chưa có(bắt đầu với giá khởi điểm)
            System.Console.WriteLine($"DescendingBidStrategy constructor called");
            // Initialize and start the timer to decrease the price every minute
            _timer = new Timer(DecreasePrice, null, TimeSpan.Zero, TimeSpan.FromSeconds(_decreaseInterval));
            _bidHub = bidHub;
        }

        public void SetUp(AuctionLotBidDto auctionLotBidDto)
        {
            System.Console.WriteLine($"SetUp called");
            _auctionLotBidDto = auctionLotBidDto;
            _currentPrice = auctionLotBidDto.StartPrice;
            _stepPrice = auctionLotBidDto.StepPercent * auctionLotBidDto.StartPrice / 100;
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

            System.Console.WriteLine($"currentPrice = {_currentPrice}");

            // Kiểm tra nếu BidAmount đạt các tiêu chí và người mua có đủ số dư
            if (bid.BidAmount == _currentPrice && bid.BidAmount <= balance)
            {
                _winner = bid.ToHighestBidLogFromCreateBidLogDto(); // Cập nhật người thắng cuộc là người đầu tiên chấp nhận giá hiện tại
                System.Console.WriteLine("61");
                if (CountdownFinished == null)
                {
                    System.Console.WriteLine("dmm");
                }
                _timer.Dispose();
                Task.Run(() => CountdownFinished!.Invoke(auctionLotBidDto.AuctionLotId)); // Kết thúc phiên đấu giá
                System.Console.WriteLine("63");
                return true;
            }

            return false;
        }

        private void DecreasePrice(object? state)
        {
            System.Console.WriteLine($"DecreasePrice called");
            if (_auctionLotBidDto != null)
            {
                System.Console.WriteLine($"Remaining time = {_auctionLotBidDto.RemainingTime}");
                _auctionLotBidDto.RemainingTime -= TimeSpan.FromSeconds(_decreaseInterval);
                _currentPrice -= _stepPrice;

                if (_auctionLotBidDto.RemainingTime.TotalSeconds <= 0 || _currentPrice <= 0)
                {
                    System.Console.WriteLine("cheets de");
                    _timer.Dispose();
                    Task.Run(() => CountdownFinished!.Invoke(_auctionLotBidDto.AuctionLotId));
                    System.Console.WriteLine("chet khong");
                }
                _bidHub.Clients.Group(_auctionLotBidDto.AuctionLotId.ToString()).SendAsync("ReceiveCurrentPrice", _currentPrice);
            }
            System.Console.WriteLine($"Updated currentPrice = {_currentPrice}");
        }
    }
}