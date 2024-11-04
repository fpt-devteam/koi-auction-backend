using AuctionService.Dto.Lot;

namespace AuctionService.IServices 
{
  public interface ILotService
  {
    Task<List<LotAuctionMethodStatisticDto>> GetLotAuctionMethodStatisticAsync();
    Task<List<BreederStatisticDto>> GetBreederStatisticsAsync(int? breederId = null);
  }
}