
using AuctionService.Dto.Lot;
using AuctionService.IRepository;
using AuctionService.IServices;
using Hangfire;

namespace AuctionService.Services
{
    public class AuctionService : IAuctionService
    {
        private const int AUCTION_STATUS_ONGOING = 2;
        private const int FIRST_AUCTION_LOT_ORDER = 1;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuctionLotService _auctionLotService;

        public AuctionService(IUnitOfWork unitOfWork, IAuctionLotService auctionLotService)
        {
            _unitOfWork = unitOfWork;
            _auctionLotService = auctionLotService;
        }

        public void ScheduleAuction(int auctionId, DateTime startTime)
        {
            TimeSpan timeToStart = startTime - DateTime.Now;
            var jobId = BackgroundJob.Schedule(() => StartAuction(auctionId, startTime), timeToStart);
        }

        public async Task StartAuction(int auctionId, DateTime startTime)
        {
            // Logic bắt đầu phiên đấu giá
            Console.WriteLine($"Auction {auctionId} is starting!");

            // Cập nhật trạng thái phiên đấu giá trong cơ sở dữ liệu
            await _unitOfWork.Auctions.UpdateStatusAsync(auctionId, AUCTION_STATUS_ONGOING);
            // Get auction lot has order in auction = 1
            var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(auctionId, FIRST_AUCTION_LOT_ORDER);

            if (auctionLot != null)
                await _auctionLotService.ScheduleAuctionLot(auctionLot.AuctionLotId, startTime);

            if (!await _unitOfWork.SaveChangesAsync())
            {
                throw new Exception("An error occurred while saving the data");
            }

            // Thông báo qua SignalR hoặc các phương thức khác
        }

    }
}
