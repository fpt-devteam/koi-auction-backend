using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.IRepository;
using AuctionService.IServices;
using Hangfire;

namespace AuctionService.Services
{
    public class AuctionLotService : IAuctionLotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int AUCTION_LOT_STATUS_ONGOING = 2;
        private const int AUCTION_LOT_STATUS_ENDED = 3;
        private const int AUCTION_STATUS_ENDED = 3;

        private const int BREAK_TIME = 5;

        public AuctionLotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void ScheduleAuctionLot(int auctionLotId, DateTime startTime)
        {
            TimeSpan timeToStart = startTime - DateTime.Now;
            var jobId = BackgroundJob.Schedule(() => StartAuctionLot(auctionLotId, startTime), timeToStart);
        }

        public async Task StartAuctionLot(int auctionLotId, DateTime startTime)
        {
            // Logic bắt đầu phiên đấu giá
            Console.WriteLine($"Auction lot {auctionLotId} is starting!");

            // Cập nhật trạng thái phiên đấu giá trong cơ sở dữ liệu
            await _unitOfWork.AuctionLots.UpdateStartTimeAsync(auctionLotId, startTime);
            await _unitOfWork.AuctionLots.UpdateStatusAsync(auctionLotId, AUCTION_LOT_STATUS_ONGOING);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }

            // Thông báo qua SignalR hoặc các phương thức khác
        }

        public async Task EndAuctionLot(int auctionLotId, DateTime endTime)
        {
            // Logic kết thúc phiên đấu giá
            Console.WriteLine($"Auction lot {auctionLotId} is ending!");

            // Cập nhật trạng thái phiên đấu giá trong cơ sở dữ liệu
            await _unitOfWork.AuctionLots.UpdateEndTimeAsync(auctionLotId, endTime);
            await _unitOfWork.AuctionLots.UpdateStatusAsync(auctionLotId, AUCTION_LOT_STATUS_ENDED);

            var currentAuctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);

            var nextAuction = await _unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(currentAuctionLot.AuctionId, currentAuctionLot.OrderInAuction + 1);
            if (nextAuction != null)
            {
                // Nếu còn AuctionLot tiếp theo thì lên lịch bắt đầu
                // start time của AuctionLot tiếp theo = end time của AuctionLot hiện tại + BREAK_TIME phút
                ScheduleAuctionLot(nextAuction.AuctionLotId, endTime.AddMinutes(BREAK_TIME));
            }
            else
            {
                // Nếu là AuctionLot cuối cùng trong phiên đấu giá thì cập nhật trạng thái phiên đấu giá
                await EndAuction(currentAuctionLot.AuctionId, endTime);
            }
            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }

            // Thông báo qua SignalR hoặc các phương thức khác
        }

        public async Task EndAuction(int auctionId, DateTime endTime)
        {
            // Logic kết thúc phiên đấu giá
            Console.WriteLine($"Auction {auctionId} is ending!");

            // Cập nhật trạng thái phiên đấu giá trong cơ sở dữ liệu
            await _unitOfWork.Auctions.UpdateStatusAsync(auctionId, AUCTION_STATUS_ENDED);
            // Cập nhật thời gian kết thúc phiên đấu giá
            await _unitOfWork.Auctions.UpdateEndTimeAsync(auctionId, endTime);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }
            // Thông báo qua SignalR hoặc các phương thức khác
        }
    }
}