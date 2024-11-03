using AuctionService.Dto.Lot;
using AuctionService.IRepository;
using AuctionService.IServices;

namespace AuctionService.Services 
{
    public class LotService : ILotService
    {
      private readonly ILotRepository _lotRepo;
      private readonly BreederDetailService _breederService;

      public LotService(ILotRepository lotRepo, BreederDetailService breederService) 
      {
        _lotRepo = lotRepo;
        _breederService = breederService;
      }

      public async Task<List<LotAuctionMethodStatisticDto>> GetLotAuctionMethodStatisticAsync()
      {
          return await _lotRepo.GetLotAuctionMethodStatisticAsync();
      }

      public async Task<List<BreederStatisticDto>> GetBreederStatisticsAsync(int? breederId = null)
      {
          var lots = await _lotRepo.GetBreederLotsStatisticsAsync(breederId);
          var statistics = new List<BreederStatisticDto>();

          var breederGroups = lots.GroupBy(l => l.BreederId);
          
          foreach (var group in breederGroups)
          {
              var breeder = await _breederService.GetBreederByIdAsync(group.Key);
              if (breeder == null) continue;

              var totalLots = group.Count();
              
              // Unsold lots are those with "Unsold" status
              var unsoldLots = group.Count(l => l.LotStatus.LotStatusName == "Unsold");
              
              // Cancelled sold lots are those with "Canceled" status AND have a SoldLot record
              var cancelledSoldLots = group.Count(l => 
                  l.LotStatus.LotStatusName == "Canceled" && 
                  l.AuctionLot != null && 
                  l.AuctionLot.SoldLot != null);
              
              // Completed lots are those with "Completed" status and have a SoldLot record
              var completedLots = group.Count(l => 
                  l.LotStatus.LotStatusName == "Completed" && 
                  l.AuctionLot != null && 
                  l.AuctionLot.SoldLot != null);

              statistics.Add(new BreederStatisticDto
              {
                  BreederId = group.Key,
                  FarmName = breeder.FarmName,
                  PercentUnsold = CalculatePercentage(unsoldLots, totalLots),
                  PercentCancelledSoldLot = CalculatePercentage(cancelledSoldLots, totalLots),
                  PercentSuccess = CalculatePercentage(completedLots, totalLots),
                  PercentUnsuccess = CalculatePercentage(unsoldLots + cancelledSoldLots, totalLots)
              });
          }

          return statistics;
      }

      private double CalculatePercentage(int part, int total)
      {
          if (total == 0) return 0;
          return Math.Round((double)part / total * 100, 2);
      }
    }
}