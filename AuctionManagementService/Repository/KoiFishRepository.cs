using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.IRepository;
using AuctionManagementService.Data;
using AuctionManagementService.Models;

namespace AuctionManagementService.Repository
{
    public class KoiFishRepository : IKoiFishRepository
    {
        private readonly AuctionManagementDbContext _context;
        public KoiFishRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<KoiFish> CreateKoiAsync(KoiFish koiFish)
        {
            await _context.KoiFishes.AddAsync(koiFish);
            await _context.SaveChangesAsync();
            return koiFish;
        }

    }
}