using AuctionManagementService.Dto.Auction;
using AuctionManagementService.Helper;
using AuctionManagementService.Models;

namespace AuctionManagementService.IRepository
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