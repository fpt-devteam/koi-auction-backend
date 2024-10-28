using AuctionService.Models;
using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;

namespace AuctionService.IRepository
{
    public interface IAuctionLotRepository
    {
        Task<List<AuctionLot>> GetAllAsync(AuctionLotQueryObject query);
        Task<AuctionLot> GetAuctionLotById(int id);
        Task<AuctionLot?> GetAuctionLotByOrderInAuction(int auctionId, int orderInAuction);

        Task<AuctionLot> CreateAsync(AuctionLot auctionLot);
        Task<List<AuctionLot>> CreateListAsync(List<AuctionLot> auctionLots);

        Task<AuctionLot> DeleteAsync(int id);
        Task<List<AuctionLot>> DeleteListAsync(List<int> ids);


        AuctionLot Update(AuctionLot auctionLot, UpdateAuctionLotDto updateAuctionLotDto);

        AuctionLot UpdateStatus(AuctionLot auctionLot, int statusId);

        AuctionLot UpdateStartTime(AuctionLot auctionLot, DateTime startTime);

        AuctionLot UpdateEndTime(AuctionLot auctionLot, DateTime endTime);

    }
}