using AuctionManagementService.Data;
using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Helper;
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

        public async Task<List<AuctionLot>> CreateListAsync(List<AuctionLot> auctionLots)
        {
            await _context.AddRangeAsync(auctionLots);
            return auctionLots;
        }

        public async Task<AuctionLot> DeleteAsync(int id)
        {
            var auctionLot = await _context.AuctionLots.
                                Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(f => f.KoiFish).ThenInclude(m => m!.KoiMedia)
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(l => l.AuctionMethod)
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(s => s.LotStatus).FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
                return null!;
            _context.Remove(auctionLot);
            return auctionLot;
        }

        public async Task<List<AuctionLot>> GetAllAsync(AuctionLotQueryObject query)
        {
            var auctionLots = await _context.AuctionLots.
            Include(a => a.AuctionLotNavigation)
                .ThenInclude(f => f.KoiFish).ThenInclude(m => m!.KoiMedia)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(l => l.AuctionMethod)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(s => s.LotStatus)
            .ToListAsync();

            if (query.AuctionId.HasValue)
            {
                auctionLots = auctionLots.Where(l => l.AuctionId == query.AuctionId.Value).ToList();
            }
            return auctionLots;
        }

        public async Task<AuctionLot> GetAuctionLotById(int id)
        {
            var auctionLot = await _context.AuctionLots.
            Include(a => a.AuctionLotNavigation)
                .ThenInclude(f => f.KoiFish).ThenInclude(m => m!.KoiMedia)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(l => l.AuctionMethod)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(s => s.LotStatus).FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
            {
                return null!;
            }
            return auctionLot;
        }

        public async Task<AuctionLot> UpdateAsync(int id, UpdateAuctionLotDto updateAuctionLotDto)
        {
            var auctionLot = await _context.AuctionLots.
                                Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(f => f.KoiFish).ThenInclude(m => m!.KoiMedia)
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(l => l.AuctionMethod)
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(s => s.LotStatus).FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
                return null!;
            auctionLot.Duration = updateAuctionLotDto.Duration;
            auctionLot.OrderInAuction = updateAuctionLotDto.OrderInAuction;
            auctionLot.StepPercent = updateAuctionLotDto.StepPercent;
            auctionLot.AuctionId = updateAuctionLotDto.AuctionId;
            return auctionLot;
        }

    }
}