using AuctionManagementService.Data;
using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.IRepository;
using AuctionManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionManagementService.Repository
{
    public class AuctionLotRepository : IAuctionLotRepository
    {
        private readonly AuctionManagementDbContext _context;
        public AuctionLotRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<AuctionLot> CreateAsync(AuctionLot auctionLot)
        {
            await _context.AddAsync(auctionLot);
            return auctionLot;
        }

        public Task<AuctionLot> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AuctionLot>> GetAllAsync()
        {
            return await _context.AuctionLots.
            Include(a => a.AuctionLotNavigation)
                .ThenInclude(f => f.KoiFish)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(l => l.AuctionMethod)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(s => s.LotStatus)
            .ToListAsync();
        }

        public async Task<AuctionLot> GetAuctionLotById(int id)
        {
            var auctionLot = await _context.AuctionLots.
            Include(a => a.AuctionLotNavigation)
                .ThenInclude(f => f.KoiFish)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(l => l.AuctionMethod)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(s => s.LotStatus).FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
            {
                return null;
            }
            return auctionLot;
        }

        public Task<AuctionLot> UpdateAsync(int id, UpdateAuctionLotDto updateAuctionLotDto)
        {
            throw new NotImplementedException();
        }

    }
}