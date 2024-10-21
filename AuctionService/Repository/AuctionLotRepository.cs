using AuctionService.Data;
using AuctionService.Models;
using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;
using AuctionService.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
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
            var auctionLot = await _context.AuctionLots.Include(a => a.AuctionLotStatus).
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

        public async Task<List<AuctionLot>> DeleteListAsync(List<int> ids)
        {
            var auctionLots = await _context.AuctionLots
                                .Where(a => ids.Contains(a.AuctionLotId))
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(f => f.KoiFish)
                                        .ThenInclude(m => m!.KoiMedia)
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(l => l.AuctionMethod)
                                .Include(a => a.AuctionLotNavigation)
                                    .ThenInclude(s => s.LotStatus).
                                Include(a => a.AuctionLotStatus)
                                    .ToListAsync();

            if (auctionLots == null)
            {
                return null!; // Không tìm thấy bất kỳ AuctionLot nào để xóa
            }
            _context.AuctionLots.RemoveRange(auctionLots);
            return auctionLots; // Trả về danh sách các AuctionLot đã xóa
        }

        public async Task<List<AuctionLot>> GetAllAsync(AuctionLotQueryObject query)
        {
            var auctionLots = await _context.AuctionLots.Include(a => a.AuctionLotStatus).
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
            var auctionLot = await _context.AuctionLots.Include(a => a.AuctionLotStatus).
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

        public AuctionLot Update(AuctionLot auctionLot, UpdateAuctionLotDto updateAuctionLotDto)
        {

            if (auctionLot == null)
                return null!;
            auctionLot.Duration = updateAuctionLotDto.Duration;
            auctionLot.OrderInAuction = updateAuctionLotDto.OrderInAuction;
            auctionLot.StepPercent = updateAuctionLotDto.StepPercent;
            auctionLot.AuctionId = updateAuctionLotDto.AuctionId;
            auctionLot.StartTime = updateAuctionLotDto.StartTime;
            auctionLot.EndTime = updateAuctionLotDto.EndTime;
            auctionLot.AuctionLotStatusId = updateAuctionLotDto.AuctionLotStatusId;

            return auctionLot;
        }

    }
}