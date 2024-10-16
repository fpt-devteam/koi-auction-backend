using AuctionManagementService.Models;
using AuctionService.Dto.Auction;
using AuctionService.Helper;
using AuctionService.Models;

namespace AuctionService.IRepository
{
    public interface IAuctionRepository
    {
        Task<List<Auction>> GetAllAsync(AuctionQueryObject queryObject);
        Task<Auction> GetByIdAsync(int id);
        Task<Auction> CreateAsync(Auction auction);
        Task<Auction> UpdateAsync(int id, UpdateAuctionDto updateAuctionDto);
        Task<Auction> DeleteAsync(int id);
    }
}