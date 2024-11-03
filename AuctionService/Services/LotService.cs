using AuctionService.Dto.Lot;
using AuctionService.IRepository;
using AuctionService.IServices;

namespace AuctionService.Services 
{
    public class LotService : ILotService
    {
      private readonly ILotRepository _lotRepo;
      public LotService(ILotRepository lotRepo) 
      {
        _lotRepo = lotRepo;
      }
        public async Task<List<LotAuctionMethodStatisticDto>> GetLotAuctionMethodStatisticAsync()
        {
            return await _lotRepo.GetLotAuctionMethodStatisticAsync();
        }
    }
}