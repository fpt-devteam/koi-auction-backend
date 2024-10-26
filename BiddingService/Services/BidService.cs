using System.Collections.Concurrent;
using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;
using BiddingService.IRepositories;
using BiddingService.IServices;
using BiddingService.Mappers;
using Microsoft.Extensions.Caching.Memory;


namespace BiddingService.Services
{
    public class BidService
    {
        private readonly ConcurrentQueue<CreateBidLogDto> _bidQueue;
        public readonly ConcurrentDictionary<int, decimal> _userBalance;
        private readonly WalletService _walletService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private decimal _standardPrice;
        private decimal _stepPrice;
        private AuctionLotDto? _auctionLotDto;
        public AuctionLotDto? AuctionLotDto
        {
            get => _auctionLotDto;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _auctionLotDto = value;
                _standardPrice = _auctionLotDto.StartPrice;
                _stepPrice = CalculateStepPrice(_auctionLotDto.StartPrice, _auctionLotDto.StepPercent);
            }
        }

        public AuctionLotBidService(IServiceScopeFactory serviceScopeFactory, WalletService walletService)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _userBalance = new ConcurrentDictionary<int, decimal>();
            _bidQueue = new ConcurrentQueue<CreateBidLogDto>();
            _walletService = walletService;
        }

        public async Task AddBidLog(CreateBidLogDto bid)
        {
            // Cập nhật _highestBid nếu bid mới lớn hơn
            _standardPrice = bid.BidAmount + _stepPrice; // Cập nhật giá trị cao nhất
            _bidQueue.Enqueue(bid);// Thêm bid hợp lệ vào hàng đợi
            await Task.Run(() => ProcessQueue());
            //await ProcessQueue();
        }

        public async Task<bool> IsBidValid(CreateBidLogDto bid)
        {
            //kiểm tra AuctionLotStaus
            if (_auctionLotDto != null && _auctionLotDto.AuctionLotId == bid.AuctionLotId
                    //&& _cacheService.GetBalance(bid.BidderId) <= bid.BidAmount
                    && bid.BidAmount >= _standardPrice)
            {
                if (!_userBalance.TryGetValue(bid.BidderId, out var balance))
                {
                    var wallet = await _walletService.GetBalanceByIdAsync(bid.BidderId);
                    balance = wallet!.Balance;
                    // Thêm balance vào Dictionary _userBalance
                    _userBalance[bid.BidderId] = balance;
                }
                if (bid.BidAmount <= _userBalance[bid.BidderId])
                    return true;
            }
            return false;
        }
        private decimal CalculateStepPrice(decimal startPrice, int stepPercent)
        {
            return startPrice * stepPercent / 100;
        }

        private async Task ProcessQueue()
        {
            while (_bidQueue.TryDequeue(out var bid))
            {
                System.Console.WriteLine($"bid = {bid.BidAmount}");
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await unitOfWork.BidLog.CreateAsync(bid.ToBidLogFromCreateBidLogDto());
                    if (!await unitOfWork.SaveChangesAsync())
                    {
                        throw new Exception("An error occurred while saving the data");
                    }
                }
            }
        }
    }
}
