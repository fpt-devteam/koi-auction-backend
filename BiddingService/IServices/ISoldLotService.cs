using BiddingService.Dto.SoldLot;
using BiddingService.Models;

namespace BiddingService.IServices
{
    public interface ISoldLotService
    {
        Task<List<SoldLot>> GetAllAsync();
        Task<SoldLot> GetSoldLotById(int id);
        Task<SoldLot> CreateSoldLot(CreateSoldLotDto soldLotDto);
    }
}