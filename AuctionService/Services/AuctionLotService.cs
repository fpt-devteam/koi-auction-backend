using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionLot;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
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

        private readonly HttpClient _httpClient;

        private const string BIDDING_SERVICE_URL = "http://localhost:3000/bidding-service";

        public AuctionLotService(IUnitOfWork unitOfWork, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
        }

        public async Task ScheduleAuctionLot(int auctionLotId, DateTime startTime)
        {
            TimeSpan timeToStart = startTime - DateTime.Now;
            var jobId = BackgroundJob.Schedule(() => StartAuctionLot(auctionLotId), timeToStart);
            // Cập nhật start time trong cơ sở dữ liệu
            await _unitOfWork.AuctionLots.UpdateStartTimeAsync(auctionLotId, startTime);

            await _unitOfWork.AuctionLotJobs.CreateAsync(new Models.AuctionLotJob { AuctionLotId = auctionLotId, HangfireJobId = jobId });
            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }
        }
        public async Task StartAuctionLot(int auctionLotId)
        {
            // Logic bắt đầu phiên đấu giá
            Console.WriteLine($"Auction lot {auctionLotId} is starting!");

            // Cập nhật status của AuctionLot trong cơ sở dữ liệu
            var auctionLot = await _unitOfWork.AuctionLots.UpdateStatusAsync(auctionLotId, AUCTION_LOT_STATUS_ONGOING);

            // Lên lịch kết thúc phiên đấu giá
            DateTime endTime = auctionLot!.StartTime!.Value.Add(auctionLot.Duration.ToTimeSpan());
            await ScheduleEndAuctionLot(auctionLotId, endTime);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }

            AuctionLotBidDto auctionLotBidDto = auctionLot!.ToAuctionLotBidDtoFromAuctionLot();

            // httpClient để gửi auctionLotBidDto đến BidService
            var response = await _httpClient.PostAsJsonAsync($"{BIDDING_SERVICE_URL}/bid/start-auction-lot", auctionLotBidDto);
        }
        public async Task ScheduleEndAuctionLot(int auctionLotId, DateTime endTime)
        {
            TimeSpan timeToEnd = endTime - DateTime.Now;
            var jobId = BackgroundJob.Schedule(() => EndAuctionLot(auctionLotId, endTime), timeToEnd);
            await _unitOfWork.AuctionLotJobs.UpdateAsync(auctionLotId, jobId);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }
        }
        public async Task EndAuctionLot(int auctionLotId, DateTime endTime)
        {
            // Logic kết thúc phiên đấu giá
            Console.WriteLine($"Auction lot {auctionLotId} is ending!");

            // Cập nhật trạng thái phiên đấu giá trong cơ sở dữ liệu
            await _unitOfWork.AuctionLots.UpdateEndTimeAsync(auctionLotId, endTime);
            await _unitOfWork.AuctionLots.UpdateStatusAsync(auctionLotId, AUCTION_LOT_STATUS_ENDED);

            var currentAuctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);

            var nextLotAuction = await _unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(currentAuctionLot.AuctionId, currentAuctionLot.OrderInAuction + 1);
            if (nextLotAuction != null)
            {
                // Nếu còn AuctionLot tiếp theo thì lên lịch bắt đầu
                // start time của AuctionLot tiếp theo = end time của AuctionLot hiện tại + BREAK_TIME phút
                var nextStartTime = endTime.AddMinutes(BREAK_TIME);
                await ScheduleAuctionLot(nextLotAuction.AuctionLotId, nextStartTime);
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

            // Notify to BiddingService
            var response = await _httpClient.PostAsync($"{BIDDING_SERVICE_URL}/bid/end-auction-lot", null);
        }
        public async Task UpdateEndTimeAuctionLot(int auctionLotId, DateTime newEndTime)
        {
            // Bước 1: Lấy thông tin AuctionLotJob từ database
            var auctionLotJob = await _unitOfWork.AuctionLotJobs.GetByAuctionLotIdAsync(auctionLotId);

            if (auctionLotJob == null)
            {
                throw new Exception($"No AuctionLotJob found for AuctionLotId {auctionLotId}");
            }

            // Bước 2: Xóa Hangfire job cũ nếu tồn tại
            if (!string.IsNullOrEmpty(auctionLotJob.HangfireJobId))
            {
                Hangfire.BackgroundJob.Delete(auctionLotJob.HangfireJobId);
            }

            // Bước 3: Tạo Hangfire job mới với thời gian mới (newEndTime)
            TimeSpan timeToEnd = newEndTime - DateTime.Now;
            var newJobId = Hangfire.BackgroundJob.Schedule(() => EndAuctionLot(auctionLotId, newEndTime), timeToEnd);

            // Bước 4: Cập nhật Job ID mới vào bảng AuctionLotJob
            // auctionLotJob.HangfireJobId = newJobId;
            await _unitOfWork.AuctionLotJobs.UpdateAsync(auctionLotId, newJobId);

            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while updating the AuctionLotJob");
            }

            // Log thông tin cập nhật thành công
            Console.WriteLine($"Updated AuctionLotId {auctionLotId} with new JobId {newJobId}");
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

/*
ScheduleAuctionLot 
    lên lịch để StartActionLot
    cập nhật startTime của AuctionLot trong cơ sở dữ liệu

ScheduleEndAuctionLot sẽ lên lịch để EndAuctionLot

StartAuctionLot sẽ cập nhật 
    status, 
    ScheduleEndAuctionLot cho chính nó
    thông báo qua SignalR -> auction lot is starting

EndAuctionLot sẽ cập nhật
    status,
    endTime của AuctionLot trong cơ sở dữ liệu
    ScheduleAuctionLot cho AuctionLot tiếp theo
    hoặc EndAuction nếu là AuctionLot cuối cùng
    thông báo qua SignalR -> auction lot is ending
*/

