using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.Dto.BidLog;
using AuctionService.Helper;
using AuctionService.IRepositories;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repositories
{
    public class BidLogRepository : IBidLogRepository
    {
        private readonly BiddingDbContext _context;
        public BidLogRepository(BiddingDbContext context)
        {
            _context = context;
        }
        public async Task<BidLog> CreateAsync(BidLog bidLog)
        {
            await _context.AddAsync(bidLog);
            return bidLog;
        }

        public async Task<List<BidLog>> GetAllAsync(BidLogQueryObject query)
        {
            var bids = await _context.BidLogs.ToListAsync();
            if (query.AuctionLotId.HasValue)
            {
                bids = bids.Where(b => b.AuctionLotId == query.AuctionLotId.Value).ToList();
            }
            return bids;
        }

        public async Task<BidLog> GetByIdAsync(int id)
        {

            var bid = await _context.BidLogs.FirstOrDefaultAsync(b => b.BidLogId == id);
            return bid ?? throw new KeyNotFoundException($"No bid found with ID: {id}"); // Ném ngoại lệ nếu không tìm thấy
        }

    }
}
