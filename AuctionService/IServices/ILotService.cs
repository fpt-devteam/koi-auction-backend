using AuctionService.Dto.Lot;
using AuctionService.Helper;

namespace AuctionService.IServices
{
  public interface ILotService
  {
    Task<List<LotAuctionMethodStatisticDto>> GetLotAuctionMethodStatisticAsync();
    Task<List<BreederStatisticDto>> GetBreederStatisticsAsync();
    Task<TotalDto> GetTotalLotsStatisticsAsync(LotQueryObject lotQuery);
    Task<List<LotSearchResultDto>> GetLotSearchResults(int breederId);

  }
}