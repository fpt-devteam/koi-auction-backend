using System.Collections.Concurrent;
using BiddingService.Dto.AuctionLot;
using BiddingService.Dto.BidLog;

namespace BiddingService.Services
{
    public class HandleMethod1
    {
        public readonly ConcurrentDictionary<int, decimal> _userBalance;
        private readonly WalletService _walletService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private decimal _standardPrice;
        private AuctionLotBidDto? _auctionLotDto;
        public AuctionLotBidDto? AuctionLotDto
        {
            get => _auctionLotDto;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _auctionLotDto = value;
                _standardPrice = _auctionLotDto.StartPrice;
            }
        }

        public HandleMethod1(IServiceScopeFactory serviceScopeFactory, WalletService walletService)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _userBalance = new ConcurrentDictionary<int, decimal>();
            _walletService = walletService;
        }
        public async Task<bool> IsBidValid(CreateBidLogDto bid)
        {
            //kiểm tra AuctionLotStaus
            if (_auctionLotDto != null && _auctionLotDto.AuctionLotId == bid.AuctionLotId
                    //&& _cacheService.GetBalance(bid.BidderId) <= bid.BidAmount
                    && bid.BidAmount == _standardPrice)
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
    }
}