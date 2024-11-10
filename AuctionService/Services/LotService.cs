using AuctionService.Dto.Dashboard;
using AuctionService.Dto.Lot;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Models;

namespace AuctionService.Services
{
    public class LotService : ILotService
    {
        private readonly int REJECT = 3;
        private readonly int UNSOLD = 5;
        private readonly int COMPLETED = 8;
        private readonly int CANCELED = 9;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BreederDetailService _breederService;

        public LotService(IUnitOfWork unitOfWork, BreederDetailService breederService)
        {
            _unitOfWork = unitOfWork;
            _breederService = breederService;
        }

        public async Task<List<LotAuctionMethodStatisticDto>> GetLotAuctionMethodStatisticAsync()
        {
            return await _unitOfWork.Lots.GetLotAuctionMethodStatisticAsync();
        }

        public async Task<List<BreederStatisticDto>> GetBreederStatisticsAsync()
        {
            var breeders = await _breederService.GetAllBreederAsync();
            var lots = await _unitOfWork.Lots.GetBreederLotsStatisticsAsync();
            var statistics = new List<BreederStatisticDto>();

            var breederGroups = lots.GroupBy(l => l.BreederId).ToDictionary(g => g.Key, g => g.ToList()); ;

            foreach (var breeder in breeders)
            {
                if (breederGroups.TryGetValue(breeder.BreederId, out var group))
                {

                    // var breeder = await _breederService.GetBreederByIdAsync(group.Key);
                    // if (breeder == null) continue;
                    // Tính các thống kê cho breeder có lot đấu giá

                    //Total auction lots
                    var totalLots = group.Count(l =>
                      l.AuctionLot != null
                    );

                    // Unsold lots are those with "Unsold" status
                    var unsoldLots = group.Count(l => l.LotStatus.LotStatusId == UNSOLD);

                    // Cancelled sold lots are those with "Canceled" status AND have a SoldLot record
                    var cancelledSoldLots = group.Count(l =>
                        l.LotStatus.LotStatusId == CANCELED &&
                        l.AuctionLot != null &&
                        l.AuctionLot.SoldLot != null);

                    // Completed lots are those with "Completed" status and have a SoldLot record
                    var completedLots = group.Count(l =>
                        l.LotStatus.LotStatusId == COMPLETED &&
                        l.AuctionLot != null &&
                        l.AuctionLot.SoldLot != null);

                    statistics.Add(new BreederStatisticDto
                    {
                        BreederId = breeder.BreederId,
                        FarmName = breeder.FarmName!,
                        TotalAuctionLot = totalLots,
                        CountSuccess = completedLots,
                        CountUnsuccess = unsoldLots + cancelledSoldLots,
                        PercentUnsold = CalculatePercentage(unsoldLots, totalLots),
                        PercentCancelledSoldLot = CalculatePercentage(cancelledSoldLots, totalLots),
                        PercentSuccess = CalculatePercentage(completedLots, totalLots),
                        PercentUnsuccess = CalculatePercentage(unsoldLots + cancelledSoldLots, totalLots),
                        Priority = GetPriorityBasedOnSuccessRate(CalculatePercentage(completedLots, totalLots))
                    });
                }
                else
                {
                    // Thêm breeder vào thống kê với các giá trị mặc định khi không có lô đấu giá
                    statistics.Add(new BreederStatisticDto
                    {
                        BreederId = breeder.BreederId,
                        FarmName = breeder.FarmName!,
                        TotalAuctionLot = 0,
                        CountSuccess = 0,
                        CountUnsuccess = 0,
                        PercentUnsold = 0,
                        PercentCancelledSoldLot = 0,
                        PercentSuccess = 0,
                        PercentUnsuccess = 0,
                        Priority = 3
                    });
                }
            }
            return statistics.OrderByDescending(s => s.PercentSuccess).ToList();
        }

        private double CalculatePercentage(int part, int total)
        {
            if (total == 0) return 0;
            return Math.Round((double)part / total * 100, 2);
        }

        // Phương thức để xác định priority dựa trên tỷ lệ thành công
        private int GetPriorityBasedOnSuccessRate(double percentSuccess)
        {
            if (percentSuccess >= 70)
                return 1;
            else if (percentSuccess >= 40)
                return 2;
            else
                return 3;
        }

        public async Task<TotalDto> GetTotalLotsStatisticsAsync(LotQueryObject lotQuery)
        {
            var lots = await _unitOfWork.Lots.GetAllAsync(lotQuery);
            var total = lots.Count;
            // var completedLots = lots.Count(l =>
            //             l.LotStatus.LotStatusId == COMPLETED &&
            //             l.AuctionLot != null &&
            //             l.AuctionLot.SoldLot != null);
            // var unsoldLots = lots.Count(l => l.LotStatus.LotStatusId == UNSOLD);
            // var cancelledSoldLots = lots.Count(l =>
            //             l.LotStatus.LotStatusId == CANCELED &&
            //             l.AuctionLot != null &&
            //             l.AuctionLot.SoldLot != null);
            // var rejectLot = lots.Count(l =>
            //             l.LotStatus.LotStatusId == REJECT);

            var completedLots = lots.Count(l =>
                                            l.LotStatus != null &&
                                            l.LotStatus.LotStatusId == COMPLETED &&
                                            l.AuctionLot != null &&
                                            l.AuctionLot.SoldLot != null);

            var unsoldLots = lots.Count(l =>
                l.LotStatus != null &&
                l.LotStatus.LotStatusId == UNSOLD);

            var cancelledSoldLots = lots.Count(l =>
                l.LotStatus != null &&
                l.LotStatus.LotStatusId == CANCELED &&
                l.AuctionLot != null &&
                l.AuctionLot.SoldLot != null);

            var rejectLot = lots.Count(l =>
                l.LotStatus != null &&
                l.LotStatus.LotStatusId == REJECT);


            // foreach (var lot in lots)
            // {
            //     var lotStatus = lot.LotStatusId;
            //     var auctionLot = lot.AuctionLot;
            //     if (auctionLot != null && auctionLot.SoldLot != null)
            //     {
            //         var soldLot = auctionLot.SoldLot;
            //         System.Console.WriteLine($"lotStatus hihi {lotStatus}");
            //     }
            // }

            // System.Console.WriteLine($"completed {completedLots}");
            // System.Console.WriteLine($"2 {unsoldLots}");
            // System.Console.WriteLine($"3 {cancelledSoldLots}");
            // System.Console.WriteLine($"rejectLot {rejectLot}");
            var result = new TotalDto
            {
                Total = total,
                CompletedLots = completedLots > 0 ? completedLots : 0,
                UnsoldLots = unsoldLots > 0 ? unsoldLots : 0,
                CanceledSoldLots = cancelledSoldLots > 0 ? cancelledSoldLots : 0,
                RejectedLots = rejectLot > 0 ? rejectLot : 0
            };

            if (result == null)
            {
                throw new InvalidOperationException("Total Statistics Fail");
            }
            return result;
        }

        public async Task<List<LotSearchResultDto>> GetLotSearchResults(int breederId)
        {
            var result = await _unitOfWork.Lots.GetLotSearchResults(breederId) ?? throw new InvalidOperationException("Not lot search existed");
            return result;
        }

        public async Task<List<DailyRevenueDto>> GetLast7DaysRevenue(int offsetWeeks)
        {
            var result = await _unitOfWork.Lots.GetLast7DaysRevenue(offsetWeeks);
            return result;
        }

        // public async Task<List<DailyRevenueDto>> GetWeeklyRevenueOfBreeders(int year, int month, int weekOfMonth)
        // {
        //     var revenue = await _unitOfWork.Lots.GetWeeklyRevenueOfBreeders(year, month, weekOfMonth);
        //     return revenue;
        // }

    }
}