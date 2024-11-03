using AuctionService.Data;
using AuctionService.Models;
using AuctionService.Dto;
using AuctionService.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
{
    public class AuctionMethodRepository : IAuctionMethodRepository
    {
        private readonly AuctionManagementDbContext _context;

        public AuctionMethodRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<AuctionMethod> CreateAsync(AuctionMethod auctionMethod)
        {
            await _context.AuctionMethods.AddAsync(auctionMethod);
            return auctionMethod;
        }

        public async Task<AuctionMethod> DeleteAsync(int id)
        {
            var method = await _context.AuctionMethods.FirstOrDefaultAsync(m => m.AuctionMethodId == id);
            if (method == null)
            {
                return null!;
            }
            _context.Remove(method);
            return method;
        }

        public async Task<List<AuctionMethod>> GetAllAsync()
        {
            return await _context.AuctionMethods.ToListAsync();
        }

        public async Task<AuctionMethod> GetByIdAsync(int id)
        {
            var method = await _context.AuctionMethods.FirstOrDefaultAsync(m => m.AuctionMethodId == id);
            if (method == null)
            {
                return null!;
            }
            return method;
        }
        public async Task<AuctionMethod> UpdateAsync(int id, UpdateAuctionMethodDto updateAuctionMethodDto)
        {
            var method = await _context.AuctionMethods.FirstOrDefaultAsync(m => m.AuctionMethodId == id);
            if (method == null)
            {
                return null!;
            }
            method.AuctionMethodName = updateAuctionMethodDto.AuctionMethodName;
            method.Description = updateAuctionMethodDto.Description;

            return method;
        }
    }
}