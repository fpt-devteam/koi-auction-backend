using System.Collections.Concurrent;
using BiddingService.Dto.BidLog;
using BiddingService.IRepositories;


namespace BiddingService.Services
{
    public class PlaceBidService
    {
        private static ConcurrentQueue<CreateBidLogDto> _bidQueue = new();
        private readonly IUnitOfWork _unitOfWork;
        private decimal _highestBid = 0;
        public PlaceBidService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //     private static ConcurrentQueue<CreateBidLogDto> _bidQueue = new();
        //     private decimal _highestBid = 0;
        //     private readonly IBidLogService _bidLogService;

        //     public PlaceBidService(IBidLogService bidLogService)
        //     {
        //         _bidLogService = bidLogService;
        //     }

        //Xử lý hàng đợi để lưu bid vào cơ sở dữ liệu
        // private async Task ProcessQueue()
        // {
        //     while (_bidQueue.TryDequeue(out var bid))
        //     {
        //         System.Console.WriteLine($"bid = {bid.BidAmount}");
        //         await _unitOfWork.BidLog.CreateAsync(bid.ToBidLogFromCreateBidLogDto());
        //         if (!await _unitOfWork.SaveChangesAsync())
        //         {
        //             throw new Exception("An error occurred while saving the data");
        //         }
        //     }
        // }

        private void ProcessQueue()
        {
            while (_bidQueue.TryDequeue(out var bid))
            {
                System.Console.WriteLine($"bid = {bid.BidAmount}");
                // await _unitOfWork.BidLog.CreateAsync(bid.ToBidLogFromCreateBidLogDto());
                // if (!await _unitOfWork.SaveChangesAsync())
                // {
                //     throw new Exception("An error occurred while saving the data");
                // }
            }
        }
        public void AddBidLog(CreateBidLogDto bid)
        {
            // Cập nhật _highestBid nếu bid mới lớn hơn
            _highestBid = bid.BidAmount; // Cập nhật giá trị cao nhất
            _bidQueue.Enqueue(bid);// Thêm bid hợp lệ vào hàng đợi
            // Task.Run(() => ProcessQueue());
            ProcessQueue();
        }
        public bool ValidateBid(CreateBidLogDto bid)
        {
            // Kiểm tra tính hợp lệ của bid
            return bid.BidAmount > _highestBid;
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

    }
}
