using System.Collections.Concurrent;
using BiddingService.Dto.BidLog;
using BiddingService.Hubs;
using BiddingService.IRepositories;
using BiddingService.IServices;
using BiddingService.Mapper;
using BiddingService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BiddingService.Services
{
    public class PlaceBidService : IPlaceBidService
    {
        private static ConcurrentQueue<CreateBidLogDto> _bidQueue = new();
        private readonly IHubContext<PlaceBidHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;
        private decimal _highestBid = 0;
        public PlaceBidService(IHubContext<PlaceBidHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
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
        private async Task ProcessQueue()
        {
            while (_bidQueue.TryDequeue(out var bid))
            {
                try
                {
                    await _unitOfWork.BidLog.CreateAsync(bid.ToBidLogFromCreateBidLogDto());
                    if (!await _unitOfWork.SaveChangesAsync())
                    {
                        throw new Exception("An error occurred while saving the data");
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        public bool PlaceBid(CreateBidLogDto bid)
        {
            // Cập nhật _highestBid nếu bid mới lớn hơn
            if (ValidateBid(bid))
            {
                _highestBid = bid.BidAmount; // Cập nhật giá trị cao nhất
                _bidQueue.Enqueue(bid);// Thêm bid hợp lệ vào hàng đợi
                Task.Run(() => ProcessQueue());
                return true;
            }
            return false;
        }
        public bool ValidateBid(CreateBidLogDto bid)
        {
            // Kiểm tra tính hợp lệ của bid
            return bid.BidAmount > _highestBid;
        }

        public async Task ProcessMessageAsync(CreateBidLogDto dto)
        {
            // Phát message đến tất cả client nếu hợp lệ bằng cách gọi phương thức BroadcastMessage từ Hub
            await _hubContext.Clients.All.SendAsync("BroadcastMessage", dto);
        }
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
