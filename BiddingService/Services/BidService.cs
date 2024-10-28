using System.Collections.Concurrent;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.BidLog;
using AuctionService.HandleMethod;
using AuctionService.IRepositories;
using AuctionService.IServices;
using AuctionService.Mappers;

namespace AuctionService.Services
{
    public class BidService
    {
        private readonly ConcurrentQueue<CreateBidLogDto> _bidQueue;
        private readonly ConcurrentDictionary<int, int> _userBalance;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private decimal _standardPrice;
        private decimal _stepPrice;

        private AuctionLotBidDto? _AuctionLotBidDto;
        public AuctionLotBidDto? AuctionLotBidDto
        {
            get => _AuctionLotBidDto;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _AuctionLotBidDto = value;
                _standardPrice = _AuctionLotBidDto.StartPrice;
                _stepPrice = CalculateStepPrice(_AuctionLotBidDto.StartPrice, _AuctionLotBidDto.StepPercent);
            }
        }

        public BidService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _userBalance = new ConcurrentDictionary<int, int>();
            _bidQueue = new ConcurrentQueue<CreateBidLogDto>();
        }

        public async Task AddBidLog(CreateBidLogDto bid)
        {
            // Cập nhật _highestBid nếu bid mới lớn hơn
            _standardPrice = bid.BidAmount + _stepPrice; // Cập nhật giá trị cao nhất
            _bidQueue.Enqueue(bid);// Thêm bid hợp lệ vào hàng đợi
            await Task.Run(() => ProcessQueue());
            //await ProcessQueue();
        }

        public bool IsBidValid(CreateBidLogDto bid)
        {
            //kiểm tra AuctionLotStaus
            if (_AuctionLotBidDto != null && _AuctionLotBidDto.AuctionLotId == bid.AuctionLotId
                    //&& _cacheService.GetBalance(bid.BidderId) <= bid.BidAmount
                    && bid.BidAmount >= _standardPrice)
            {
                return true;
            }
            return false;
        }
        private decimal CalculateStepPrice(decimal startPrice, int stepPercent)
        {
            return startPrice * stepPercent / 100;
        }
        // // Ví dụ về phương thức kiểm tra tính hợp lệ
        //         private Task<bool> ValidateBidMessage(CreateBidLogDto bidMessage)
        //         {
        //             // Logic kiểm tra tính hợp lệ
        //             // Ví dụ: Kiểm tra số tiền đấu giá có lớn hơn mức tối thiểu không
        //             if (bidMessage.Amount <= 0)
        //             {
        //                 return Task.FromResult(false);
        //             }

        //             // Kiểm tra các logic khác liên quan đến phiên đấu giá, thời gian, v.v.
        //             // Ví dụ: kiểm tra nếu phiên đấu giá đã kết thúc
        //             var auctionIsActive = CheckAuctionStatus(bidMessage.AuctionLotId);
        //             if (!auctionIsActive)
        //             {
        //                 return Task.FromResult(false);
        //             }

        //             // Nếu tất cả các kiểm tra đều hợp lệ, trả về true
        //             return Task.FromResult(true);
        //         }

        //         // Ví dụ về phương thức kiểm tra trạng thái phiên đấu giá
        //         private bool CheckAuctionStatus(int roomId)
        //         {
        //             // Thực hiện kiểm tra xem phiên đấu giá có đang hoạt động không
        //             // (dựa trên roomId hoặc logic cụ thể của dự án)
        //             // Trả về true nếu phiên đấu giá đang hoạt động, ngược lại false
        //             return true; // Đây chỉ là ví dụ, bạn cần thay bằng logic thực tế
        //         }
        // public async Task ProcessMessageAsync(CreateBidLogDto dto)
        // {
        //     // Phát message đến tất cả client nếu hợp lệ bằng cách gọi phương thức BroadcastMessage từ Hub
        //     await _hubContext.Clients.All.SendAsync("BroadcastMessage", dto);
        // }
        //     public void EnqueueBid(CreateBidLogDto bid)
        //     {
        //         _bidQueue.Enqueue(bid);
        //     }

        //     public async Task ProcessQueue()
        //     {
        //         while (_bidQueue.TryDequeue(out var bid))
        //         {
        //             try
        //             {
        //                 await _bidLogService.CreateBidLog(bid.ToBidLogFromCreateBidLogDto());
        //             }
        //             catch (Exception)
        //             {
        //                 throw;
        //             }

        //         }
        //     }

        //     public Task SendPlaceBidAsync(CreateBidLogDto message)
        //     {
        //         throw new NotImplementedException();
        //     }

        //     public Task<List<BidLog>> GetAllAsync()
        //     {
        //         throw new NotImplementedException();
        //     }
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
