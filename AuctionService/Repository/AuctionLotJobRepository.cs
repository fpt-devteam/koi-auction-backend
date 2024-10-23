using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.IRepository;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
{

    public class AuctionLotJobRepository : IAuctionLotJobRepository
    {
        private readonly AuctionManagementDbContext _context;
        public AuctionLotJobRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<AuctionLotJob?> CreateAsync(AuctionLotJob auctionLotJob)
        {
            await _context.AddAsync(auctionLotJob);
            return auctionLotJob;
        }

        public async Task<AuctionLotJob?> GetByAuctionLotIdAsync(int auctionLotId)
        {
            return await _context.AuctionLotJobs.FirstOrDefaultAsync(a => a.AuctionLotId == auctionLotId);
        }

        public async Task<AuctionLotJob?> UpdateAsync(int auctionLotId, string? hangfireJobId = null)
        {
            var auctionLotJob = await _context.AuctionLotJobs.FirstOrDefaultAsync(a => a.AuctionLotId == auctionLotId);
            if (auctionLotJob == null)
                return null;
            auctionLotJob.HangfireJobId = hangfireJobId;
            return auctionLotJob;
        }
    }
}