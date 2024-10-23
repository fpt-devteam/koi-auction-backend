using AuctionService.Models;
using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;

namespace AuctionService.IRepository
{
    public interface IAuctionLotJobRepository
    {
        // Lấy AuctionLotJob dựa trên AuctionLotId
        Task<AuctionLotJob?> GetByAuctionLotIdAsync(int auctionLotId);

        // Thêm AuctionLotJob mới
        Task<AuctionLotJob?> CreateAsync(AuctionLotJob auctionLotJob);

        // Cập nhật AuctionLotJob (ví dụ như cập nhật HangfireJobId)
        Task<AuctionLotJob?> UpdateAsync(int auctionLotId, string? hangfireJobId = null);

    }
}