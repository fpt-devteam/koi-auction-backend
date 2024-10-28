using AuctionService.Data;
using AuctionService.IRepository;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
{
    public class SoldLotRepository : ISoldLotRepository
    {
        private readonly AuctionManagementDbContext _context;
        public SoldLotRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<SoldLot> CreateSoldLot(SoldLot soldLot)
        {
            await _context.AddAsync(soldLot);
            return soldLot;
        }

        public async Task<List<SoldLot>> GetAllAsync()
        {
            var soldLots = await _context.SoldLots.ToListAsync();
            if (soldLots == null)
                throw new Exception("SoldLots is null");
            return soldLots;
        }

        public async Task<SoldLot> GetSoldLotById(int id)
        {
            var soldLot = await _context.SoldLots.FirstOrDefaultAsync(s => s.SoldLotId == id);
            if (soldLot == null)
            {
                throw new Exception("Sold Lot is not existed");
            }
            return soldLot;
        }

    }
}