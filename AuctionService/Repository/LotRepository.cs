using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Models;
using AuctionService.Dto.Lot;
using AuctionService.Dto.LotRequestForm;
using AuctionService.IRepository;
using Microsoft.EntityFrameworkCore;
using AuctionService.Helper;
using AuctionService.Data;

namespace AuctionService.Repository
{
    public class LotRepository : ILotRepository
    {
        private readonly AuctionManagementDbContext _context;

        public LotRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<Lot> CreateLotAsync(Lot lot)
        {
            var status = await _context.LotStatuses
                                     .FirstOrDefaultAsync(ls => ls.LotStatusName == "Pending");
            lot.LotStatusId = status!.LotStatusId;
            await _context.LotStatuses.FindAsync(lot.LotStatusId);
            await _context.AuctionMethods.FindAsync(lot.AuctionMethodId);
            await _context.Lots.AddAsync(lot);

            return lot;
        }

        public async Task<Lot> DeleteLotAsync(int id)
        {
            var lot = await _context.Lots.FirstOrDefaultAsync(l => l.LotId == id);
            if (lot == null)
                throw new KeyNotFoundException($" Lot {id} was not found");
            var status = await _context.LotStatuses
                                  .FirstOrDefaultAsync(ls => ls.LotStatusName == "Canceled");
            if (status == null) throw new Exception("status not existed");
            lot.LotStatusId = status!.LotStatusId;
            return lot;
        }


        public async Task<List<Lot>> GetAllAsync(LotQueryObject query)
        {
            var lots = _context.Lots.Include(l => l.KoiFish).ThenInclude(m => m!.KoiMedia).
                                        Include(l => l.LotStatus).
                                        Include(l => l.AuctionMethod).AsQueryable();

            // Kiểm tra BreederId
            if (query.BreederId.HasValue)
            {
                lots = lots.Where(l => l.BreederId == query.BreederId.Value);
            }

            // Kiểm tra StartingPrice
            if (query.StartingPrice.HasValue)
            {
                lots = lots.Where(l => l.StartingPrice >= query.StartingPrice.Value);
            }

            // Kiểm tra LotStatusId
            if (query.LotStatusId.HasValue)
            {
                lots = lots.Where(l => l.LotStatusId == query.LotStatusId.Value);
            }

            // Kiểm tra AuctionMethodId
            if (query.AuctionMethodId.HasValue)
            {
                lots = lots.Where(l => l.AuctionMethodId == query.AuctionMethodId.Value);
            }

            // Thực hiện tìm kiếm theo các thuộc tính của KoiFish
            if (query.Sex.HasValue)
            {
                lots = lots.Where(l => l.KoiFish!.Sex == query.Sex.Value);
            }

            if (query.MinWeightKg.HasValue)
            {
                lots = lots.Where(l => l.KoiFish!.WeightKg >= query.MinWeightKg.Value);
            }

            if (query.MaxWeightKg.HasValue)
            {
                lots = lots.Where(l => l.KoiFish!.WeightKg <= query.MaxWeightKg.Value);
            }

            if (query.MinSizeCm.HasValue)
            {
                lots = lots.Where(l => l.KoiFish!.SizeCm >= query.MinSizeCm.Value);
            }

            if (query.MaxSizeCm.HasValue)
            {
                lots = lots.Where(l => l.KoiFish!.SizeCm <= query.MaxSizeCm.Value);
            }

            if (query.YearOfBirth.HasValue)
            {
                lots = lots.Where(l => l.KoiFish!.YearOfBirth == query.YearOfBirth.Value);
            }
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
                {
                    case "startingprice":
                        lots = query.IsDescending
                            ? lots.OrderByDescending(l => l.StartingPrice)
                            : lots.OrderBy(l => l.StartingPrice);
                        break;

                    case "createdat":
                        lots = query.IsDescending
                            ? lots.OrderByDescending(l => l.CreatedAt)
                            : lots.OrderBy(l => l.CreatedAt);
                        break;
                    case "updatedat":
                        lots = query.IsDescending
                            ? lots.OrderByDescending(l => l.UpdatedAt)
                            : lots.OrderBy(l => l.UpdatedAt);
                        break;

                    case "sizecm":
                        lots = query.IsDescending
                            ? lots.OrderByDescending(l => l.KoiFish!.SizeCm)
                            : lots.OrderBy(l => l.KoiFish!.SizeCm);
                        break;

                    case "weightkg":
                        lots = query.IsDescending
                            ? lots.OrderByDescending(l => l.KoiFish!.WeightKg)
                            : lots.OrderBy(l => l.KoiFish!.WeightKg);
                        break;

                    default:
                        // Xử lý trường hợp không khớp với bất kỳ giá trị SortBy nào
                        break;
                }
            }

            return await lots.ToListAsync();
        }

        public async Task<Lot> GetLotByIdAsync(int id)
        {
            var lot = await _context.Lots.Include(l => l.KoiFish).ThenInclude(m => m!.KoiMedia).
                                            Include(l => l.LotStatus).
                                            Include(l => l.AuctionMethod).FirstOrDefaultAsync(l => l.LotId == id);
            if (lot == null)
                throw new KeyNotFoundException($" Lot {id} was not found");
            return lot;
        }

        public async Task<Lot> UpdateLotAsync(int id, UpdateLotDto updateLotDto)
        {
            var lot = await _context.Lots.FirstOrDefaultAsync(l => l.LotId == id);
            if (lot == null)
                throw new KeyNotFoundException($" Lot {id} was not found");

            var koiFish = lot.KoiFish;
            lot.StartingPrice = updateLotDto.StartingPrice;
            lot.AuctionMethodId = updateLotDto.AuctionMethodId;
            await _context.AuctionMethods.FindAsync(lot.AuctionMethodId);
            await _context.LotStatuses.FindAsync(lot.LotStatusId);
            return lot;
        }

        public async Task<Lot> UpdateLotStatusAsync(int id, UpdateLotStatusDto updateLot)
        {
            var lot = await _context.Lots.FirstOrDefaultAsync(l => l.LotId == id);
            if (lot == null)
                throw new KeyNotFoundException($" Lot {id} was not found");
            var status = await _context.LotStatuses.FirstOrDefaultAsync(x => x.LotStatusName == updateLot.LotStatusName);
            if (status == null)
                throw new Exception("status not existed");
            lot.LotStatusId = status!.LotStatusId;
            return lot;
        }
    }
}