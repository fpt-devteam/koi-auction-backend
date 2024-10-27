using BiddingService.Models;

namespace BiddingService.IRepositories
{
    public interface ISoldLotRepository
    {
        Task<List<SoldLot>> GetAllAsync();
        Task<SoldLot> GetSoldLotById(int auctionLotId);
        Task<SoldLot> CreateSoldLot(SoldLot soldLot);

    }
}