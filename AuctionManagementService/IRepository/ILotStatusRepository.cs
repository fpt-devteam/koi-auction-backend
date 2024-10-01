using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Dto.LotStatus;
using AuctionManagementService.Models;

namespace AuctionManagementService.IRepository
{
    public interface ILotStatusRepository
    {
         Task<List<LotStatus>> GetAllAsync();
        Task<LotStatus> GetLotStatusByIdAsync(int id);
        Task<LotStatus> CreateLotStatusAsync(LotStatus LotStatus);
        Task<LotStatus> UpdateLotStatusAsync(int id, UpdateLotStatusDto lotStatusDto);
        Task<LotStatus> DeleteLotStatusAsync(int id);
    }
}