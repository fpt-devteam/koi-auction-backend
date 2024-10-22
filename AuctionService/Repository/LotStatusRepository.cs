using AuctionService.Data;
using AuctionService.Models;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.LotStatus;
using AuctionService.IRepository;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
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

            return LotStatus;
        }

        public async Task<LotStatus> DeleteLotStatusAsync(int id)
        {
            var lotStatus = await _context.LotStatuses.FirstOrDefaultAsync(l => l.LotStatusId == id);
            if (lotStatus == null)
            {
                return null!;
            }
            _context.Remove(lotStatus);

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

        public async Task<LotStatus> UpdateLotStatusAsync(int id, UpdateStatusDto lotStatusDto)
        {
            var lotStatus = await _context.LotStatuses.FirstOrDefaultAsync(l => l.LotStatusId == id);
            if (lotStatus == null)
            {
                return null!;
            }
            lotStatus.LotStatusName = lotStatusDto.LotStatusName!;

            return lotStatus;
        }

    }
}