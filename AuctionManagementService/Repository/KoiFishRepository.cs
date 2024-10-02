using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.IRepository;
using AuctionManagementService.Data;
using AuctionManagementService.Models;
using AuctionManagementService.Dto.KoiFish;
using Microsoft.EntityFrameworkCore;

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
            
            return koiFish;
        }

        public async Task<KoiFish> UpdateKoiAsync(int id, UpdateKoiFishDto updateKoiDto)
        {
            var koiFish = await _context.KoiFishes.FirstOrDefaultAsync(f => f.KoiFishId == id);
            if (koiFish == null)
            {
                return null!;
            }
            koiFish.Variety = updateKoiDto.Variety;
            koiFish.Sex = updateKoiDto.Sex;
            koiFish.SizeCm = updateKoiDto.SizeCm;
            koiFish.YearOfBirth = updateKoiDto.YearOfBirth;
            koiFish.WeightKg = updateKoiDto.WeightKg;
            return koiFish;
        }
    }
}