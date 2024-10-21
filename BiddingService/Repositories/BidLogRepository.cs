using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Data;
using BiddingService.Dto.BidLog;
using BiddingService.IRepositories;
using BiddingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Repositories
{
    public class BidLogRepository : IBidLogRepository
    {
        private readonly BiddingDbContext _context;
        public BidLogRepository(BiddingDbContext context)
        {
            _context = context;
        }
        public async Task<BidLog> CreateAsync(BidLog bigLog)
        {
            await _context.AddAsync(bigLog);
            return bigLog;
        }

        public async Task<List<BidLog>> GetAllAsync()
        {
            var bids = await _context.BidLogs.ToListAsync();
            return bids;
        }

        public async Task<BidLog> GetByIdAsync(int id)
        {

            var bid = await _context.BidLogs.FirstOrDefaultAsync(b => b.BidLogId == id);
            return bid ?? throw new KeyNotFoundException($"No bid found with ID: {id}"); // Ném ngoại lệ nếu không tìm thấy
        }

        public async Task<MaxBidAmountDto> GetMaxAmountByAuctionLotIdAsync(int auctionLotId)
        {

            var bid = await _context.BidLogs
                .Where(b => b.AuctionLotId == auctionLotId)
                .GroupBy(b => b.AuctionLotId)
                .Select(g => new MaxBidAmountDto
                {
                    AuctionLotId = g.Key,
                    MaxBidAmount = g.Max(b => b.BidAmount)
                })
                .FirstOrDefaultAsync();

            return bid!; // Trả về kết quả (có thể là null nếu không có bid nào)
        }

    }
}
