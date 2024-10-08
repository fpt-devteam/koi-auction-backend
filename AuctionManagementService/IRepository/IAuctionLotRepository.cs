using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Helper;
using AuctionManagementService.Models;

namespace AuctionManagementService.IRepository
{
    public interface IAuctionLotRepository
    {
        Task<List<AuctionLot>> GetAllAsync(AuctionLotQueryObject query);
        Task<AuctionLot> GetAuctionLotById(int id);
        Task<AuctionLot> CreateAsync(AuctionLot auctionLot);
        Task<List<AuctionLot>> CreateListAsync(List<AuctionLot> auctionLots);
        Task<AuctionLot> UpdateAsync(int id, UpdateAuctionLotDto updateAuctionLotDto);
        Task<AuctionLot> DeleteAsync(int id);

    }
}