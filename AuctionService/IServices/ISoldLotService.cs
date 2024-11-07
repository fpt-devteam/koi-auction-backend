using AuctionService.Dto.SoldLot;
using AuctionService.Helper;
using AuctionService.Models;

namespace AuctionService.IServices
{
    public interface ISoldLotService
    {
        Task<List<SoldLot>> GetAllAsync(SoldLotQueryObject queryObject);
        Task<SoldLot> GetSoldLotById(int id);
        Task<SoldLot> CreateSoldLot(CreateSoldLotDto soldLotDto);
    }
}