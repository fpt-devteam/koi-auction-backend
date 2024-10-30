using AuctionService.Data;
using AuctionService.Models;
using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;
using AuctionService.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repository
{
    public class AuctionLotRepository : IAuctionLotRepository
    {
        private readonly AuctionManagementDbContext _context;
        public AuctionLotRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<AuctionLot> CreateAsync(AuctionLot auctionLot)
        {
            await _context.AddAsync(auctionLot);
            return auctionLot;
        }

        public async Task<List<AuctionLot>> CreateListAsync(List<AuctionLot> auctionLots)
        {
            await _context.AddRangeAsync(auctionLots);
            return auctionLots;
        }

        public async Task<AuctionLot> DeleteAsync(int id)
        {
            var auctionLot = await _context.AuctionLots.FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
                throw new KeyNotFoundException($"Auction Lot {id} was not found");
            _context.Remove(auctionLot);
            return auctionLot;
        }

        public async Task<List<AuctionLot>> DeleteListAsync(List<int> ids)
        {
            var auctionLots = await _context.AuctionLots.Where(a => ids.Contains(a.AuctionLotId) && a.AuctionLotStatusId == (int)Enums.AuctionLotStatus.Upcoming).ToListAsync();

            if (auctionLots.Count == 0)
            {
                throw new KeyNotFoundException($"Auction Lot was not found or auction lot status is not upcomming"); // Không tìm thấy bất kỳ AuctionLot nào để xóa
            }
            if (auctionLots.Count != ids.Count)
            {
                var notFoundIds = ids.Except(auctionLots.Select(al => al.AuctionLotId)).ToList();
                throw new KeyNotFoundException($"Auction Lots with the following IDs were not found: {string.Join(", ", notFoundIds)}");
            }

            _context.AuctionLots.RemoveRange(auctionLots);
            return auctionLots; // Trả về danh sách các AuctionLot đã xóa
        }

        public async Task<List<AuctionLot>> GetAllAsync(AuctionLotQueryObject query)
        {
            var auctionLots = await _context.AuctionLots.Include(a => a.AuctionLotStatus).
                                        Include(a => a.AuctionLotNavigation)
                                            .ThenInclude(f => f.KoiFish).ThenInclude(m => m!.KoiMedia)
                                        .Include(a => a.AuctionLotNavigation)
                                            .ThenInclude(l => l.AuctionMethod)
                                        .Include(a => a.AuctionLotNavigation)
                                            .ThenInclude(s => s.LotStatus)
                                        .ToListAsync();

            if (query.AuctionId.HasValue)
            {
                auctionLots = auctionLots.Where(l => l.AuctionId == query.AuctionId.Value).ToList();
            }
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
                {
                    case "orderinauction":
                        // Sắp xếp tăng dần OrderInAuction
                        auctionLots = auctionLots.OrderBy(l => l.OrderInAuction).ToList();
                        break;
                    default:
                        // Xử lý trường hợp không khớp với bất kỳ giá trị SortBy nào
                        break;
                }
            }
            return auctionLots;
        }

        public async Task<AuctionLot> GetAuctionLotById(int id)
        {
            var auctionLot = await _context.AuctionLots.Include(a => a.AuctionLotStatus).
            Include(a => a.AuctionLotNavigation)
                .ThenInclude(f => f.KoiFish).ThenInclude(m => m!.KoiMedia)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(l => l.AuctionMethod)
            .Include(a => a.AuctionLotNavigation)
                .ThenInclude(s => s.LotStatus).FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
            {
                throw new KeyNotFoundException($"Auction Lot {id} was not found");
            }
            return auctionLot;
        }

        public AuctionLot Update(AuctionLot auctionLot, UpdateAuctionLotDto updateAuctionLotDto)
        {

            if (auctionLot == null)
                throw new KeyNotFoundException($"Auction Lot {auctionLot!.AuctionLotId} was not found");
            auctionLot.Duration = updateAuctionLotDto.Duration;
            auctionLot.OrderInAuction = updateAuctionLotDto.OrderInAuction;
            auctionLot.StepPercent = updateAuctionLotDto.StepPercent;
            auctionLot.AuctionId = updateAuctionLotDto.AuctionId;
            auctionLot.StartTime = updateAuctionLotDto.StartTime;
            auctionLot.EndTime = updateAuctionLotDto.EndTime;
            auctionLot.AuctionLotStatusId = updateAuctionLotDto.AuctionLotStatusId;

            return auctionLot;
        }

        public async Task<AuctionLot?> UpdateStatusAsync(int id, int statusId)
        {
            var auctionLot = await GetAuctionLotById(id);
            if (auctionLot == null)
                throw new KeyNotFoundException($"Auction Lot {id} was not found");
            auctionLot.AuctionLotStatusId = statusId;
            return auctionLot;
        }

        public async Task<AuctionLot?> UpdateStartTimeAsync(int id, DateTime startTime)
        {
            var auctionLot = await _context.AuctionLots.FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
                throw new KeyNotFoundException($"Auction Lot {id} was not found");
            auctionLot.StartTime = startTime;
            return auctionLot;
        }

        public async Task<AuctionLot?> GetAuctionLotByOrderInAuction(int auctionId, int orderInAuction)
        {
            var auctionLot = await _context.AuctionLots.FirstOrDefaultAsync(a => a.AuctionId == auctionId && a.OrderInAuction == orderInAuction);
            if (auctionLot == null)
                throw new KeyNotFoundException($"Auction Lot {auctionLot!.AuctionLotId} was not found");
            return auctionLot;
        }

        public async Task<AuctionLot?> UpdateEndTimeAsync(int id, DateTime endTime)
        {
            var auctionLot = await _context.AuctionLots.FirstOrDefaultAsync(a => a.AuctionLotId == id);
            if (auctionLot == null)
                throw new KeyNotFoundException($"Auction Lot {auctionLot!.AuctionLotId} was not found");
            auctionLot.EndTime = endTime;
            return auctionLot;
        }
    }
}