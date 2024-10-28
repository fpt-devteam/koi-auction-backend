using AuctionService.Dto.SoldLot;
using AuctionService.Models;

namespace AuctionService.IServices
{
    public interface ISoldLotService
    {
        Task<List<SoldLot>> GetAllAsync();
        Task<SoldLot> GetSoldLotById(int id);
        Task<SoldLot> CreateSoldLot(CreateSoldLotDto soldLotDto);
    }
}