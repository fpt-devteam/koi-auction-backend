using System.ComponentModel.DataAnnotations;
using AuctionManagementService.Dto.Lot;

namespace AuctionManagementService.Dto.AuctionLot
{
    public class CreateAuctionLotDto
    {
        [Required]
        public TimeOnly Duration { get; set; }
        [Required]
        public int OrderInAuction { get; set; }
        [Required]
        public int StepPercent { get; set; }
        [Required]
        public int AuctionLotId { get; set; }
        [Required]
        public int AuctionId { get; set; }
    }
}