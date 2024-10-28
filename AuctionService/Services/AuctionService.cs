
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
        // private readonly IAuctionLotService _auctionLotService;

        // public AuctionService(IUnitOfWork unitOfWork, IAuctionLotService auctionLotService)
        // {
        //     _unitOfWork = unitOfWork;
        //     _auctionLotService = auctionLotService;
        // }

        public AuctionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void ScheduleAuction(int auctionId, DateTime startTime)
        {

            TimeSpan timeToStart = startTime - DateTime.Now;
            if (timeToStart <= TimeSpan.Zero)
            {
                Console.WriteLine("Start time is in the past. Auction cannot be scheduled.");
            }

            // // Lên lịch cho phiên đấu giá
            BackgroundJob.Schedule(() => StartAuctionWrapper(auctionId, startTime), timeToStart);  // BackgroundJob.Schedule(async () => await StartAuctionAsync(auctionId, startTime), timeToStart);

            // // Lấy auction lot có thứ tự đầu tiên trong phiên đấu giá
            // var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(auctionId, FIRST_AUCTION_LOT_ORDER);

            // // Kiểm tra nếu không tìm thấy auction lot
            // if (auctionLot == null)
            // {
            //     throw new Exception($"Auction Lot with order {FIRST_AUCTION_LOT_ORDER} not found for Auction {auctionId}");
            // }

            // Lên lịch cho lot đấu giá đầu tiên
            // await _auctionLotService.ScheduleAuctionLot(auctionLot.AuctionLotId, startTime);
            System.Console.WriteLine($"Scheduled Auction {auctionId} at {startTime}");
        }

        public void StartAuctionWrapper(int auctionId, DateTime startTime)
        {
            Task.Run(() => StartAuctionAsync(auctionId, startTime)).Wait();
        }
        public async Task StartAuctionAsync(int auctionId, DateTime startTime)
        {
            try
            {
                // Log bắt đầu phiên đấu giá
                Console.WriteLine($"Auction {auctionId} is starting!");

                // Cập nhật trạng thái phiên đấu giá trong cơ sở dữ liệu
                await _unitOfWork.Auctions.UpdateStatusAsync(auctionId, AUCTION_STATUS_ONGOING);

                // // Lấy auction lot có thứ tự đầu tiên trong phiên đấu giá
                // var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotByOrderInAuction(auctionId, FIRST_AUCTION_LOT_ORDER);

                // // Kiểm tra nếu không tìm thấy auction lot
                // if (auctionLot == null)
                // {
                //     throw new Exception($"Auction Lot with order {FIRST_AUCTION_LOT_ORDER} not found for Auction {auctionId}");
                // }

                // // Lên lịch cho lot đấu giá đầu tiên
                // await _auctionLotService.ScheduleAuctionLot(auctionLot.AuctionLotId, startTime);
                // Lưu thay đổi vào cơ sở dữ liệu
                if (!await _unitOfWork.SaveChangesAsync())
                {
                    throw new Exception("An error occurred while saving the auction data.");
                }


                // Log thành công
                Console.WriteLine($"Auction {auctionId} has started successfully.");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Failed to start Auction {auctionId}: {ex.Message}");
                // Có thể dùng ILogger để log thêm thông tin chi tiết hơn hoặc lưu log vào tệp
                // throw;
            }
        }
    }
}
