using AuctionService.Dto;
using AuctionService.Models;

namespace AuctionService.IRepository
{
    public interface IAuctionMethodRepository
    {
        Task<List<AuctionMethod>> GetAllAsync();
        Task<AuctionMethod> GetByIdAsync(int id);
        Task<AuctionMethod> CreateAsync(AuctionMethod auctionMethod);
        Task<AuctionMethod> UpdateAsync(int id, UpdateAuctionMethodDto updateAuctionMethodDto);
        Task<AuctionMethod> DeleteAsync(int id);
    }
}