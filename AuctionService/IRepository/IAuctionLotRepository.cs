using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;
using AuctionService.Models;

namespace AuctionService.IRepository
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