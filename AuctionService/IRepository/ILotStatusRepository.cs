using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.LotStatus;
using AuctionService.Models;

namespace AuctionService.IRepository
{
    public interface ILotStatusRepository
    {
        Task<List<LotStatus>> GetAllAsync();
        Task<LotStatus> GetLotStatusByIdAsync(int id);
        Task<LotStatus> CreateLotStatusAsync(LotStatus LotStatus);
        Task<LotStatus> UpdateLotStatusAsync(int id, UpdateStatusDto lotStatusDto);
        Task<LotStatus> DeleteLotStatusAsync(int id);
    }
}