using AuctionManagementService.Data;
using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Dto.LotStatus;
using AuctionManagementService.IRepository;
using AuctionManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionManagementService.Repository
{
    public class LotStatusRepository : ILotStatusRepository
    {
        private readonly AuctionManagementDbContext _context;
        public LotStatusRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<LotStatus> CreateLotStatusAsync(LotStatus LotStatus)
        {
            await _context.AddAsync(LotStatus);
            await _context.SaveChangesAsync();
            return LotStatus;
        }

        public async Task<LotStatus> DeleteLotStatusAsync(int id)
        {
            var lotStatus = await _context.LotStatuses.FirstOrDefaultAsync(l => l.LotStatusId == id);
            if (lotStatus == null)
            {
                return null;
            }
            _context.Remove(lotStatus);
            await _context.SaveChangesAsync();
            return lotStatus;
        }

        public async Task<List<LotStatus>> GetAllAsync()
        {
            return await _context.LotStatuses.ToListAsync();
        }

        public async Task<LotStatus> GetLotStatusByIdAsync(int id)
        {
            return await _context.LotStatuses.FirstOrDefaultAsync(l => l.LotStatusId == id);
        }

        public async Task<LotStatus> UpdateLotStatusAsync(int id, UpdateLotStatusDto lotStatusDto)
        {
            var lotStatus = await _context.LotStatuses.FirstOrDefaultAsync(l => l.LotStatusId == id);
            if (lotStatus == null)
            {
                return null;
            }
            lotStatus.LotStatusName = lotStatusDto.LotStatusName;
            await _context.SaveChangesAsync();
            return lotStatus;
        }

    }
}