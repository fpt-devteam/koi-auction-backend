using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Models;

namespace AuctionManagementService.IRepository
{
    public interface IAuctionLotRepository
    {
        Task<List<AuctionLot>> GetAllAsync();
        Task<AuctionLot> GetAuctionLotById(int id);
        Task<AuctionLot> CreateAsync(AuctionLot auctionLot);
         Task<AuctionLot> UpdateAsync(int id, UpdateAuctionLotDto updateAuctionLotDto);
         Task<AuctionLot> DeleteAsync(int id);
         
    }
}