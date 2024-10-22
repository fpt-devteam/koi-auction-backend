using System.ComponentModel.DataAnnotations;

namespace AuctionService.Dto.AuctionLot
{
    public class UpdateAuctionLotDto
    {
        [Required]
        public TimeOnly Duration { get; set; }
        [Required]
        public int OrderInAuction { get; set; }

        public int? StepPercent { get; set; }
        [Required]
        public int AuctionId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}