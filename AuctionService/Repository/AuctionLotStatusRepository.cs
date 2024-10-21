using AuctionService.Data;
using AuctionService.IRepository;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
{
    public class AuctionLotStatusRepository : IAuctionLotStatusRepository
    {
        private readonly AuctionManagementDbContext _context;
        public AuctionLotStatusRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<List<AuctionLotStatus>> GetAllAsync()
        {
            return await _context.AuctionLotStatuses.ToListAsync();
        }

        public async Task<AuctionLotStatus> GetAuctionLotStatusByIdAsync(int id)
        {
            var status = await _context.AuctionLotStatuses.FirstOrDefaultAsync(a => a.AuctionLotStatusId == id);
            if (status == null)
            {
                throw new ArgumentException("status not existed");
            }
            return status;
        }

    }
}