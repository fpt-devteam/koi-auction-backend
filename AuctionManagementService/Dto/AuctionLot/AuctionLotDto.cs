using AuctionManagementService.Dto.Lot;

namespace AuctionManagementService.Dto.AuctionLot
{
    public class AuctionLotDto
    {
        public int AuctionId { get; set; }
        public TimeOnly Duration { get; set; }
        public int OrderInAuction { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StepPercent { get; set; }
        public DateTime? EndTime { get; set; }
        public LotDto? LotDto { get; set; }
    }
}