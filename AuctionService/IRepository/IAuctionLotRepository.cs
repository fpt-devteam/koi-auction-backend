using AuctionService.Models;
using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;

namespace AuctionService.IRepository
{
    public interface IAuctionLotRepository
    {
        Task<List<AuctionLot>> GetAllAsync(AuctionLotQueryObject query);
        Task<AuctionLot> GetAuctionLotById(int id);
        Task<AuctionLot> CreateAsync(AuctionLot auctionLot);
        Task<List<AuctionLot>> CreateListAsync(List<AuctionLot> auctionLots);
        AuctionLot Update(AuctionLot auctionLot, UpdateAuctionLotDto updateAuctionLotDto);
        Task<AuctionLot> DeleteAsync(int id);
        Task<List<AuctionLot>> DeleteListAsync(List<int> ids);

    }
}