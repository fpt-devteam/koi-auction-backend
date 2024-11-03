using AuctionService.Models;

namespace AuctionService.IRepository
{
    public interface ISoldLotRepository
    {
        Task<List<SoldLot>> GetAllAsync();
        Task<SoldLot> GetSoldLotById(int auctionLotId);
        Task<SoldLot> CreateSoldLot(SoldLot soldLot);

    }
}