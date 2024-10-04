using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.AuctionLot
{
    public class UpdateAuctionLotDto
    {
        [Required]
        public TimeOnly Duration { get; set; }
        [Required]
        public int OrderInAuction { get; set; }
        [Required]
        public int StepPercent { get; set; }
        
        [Required]
        public int AuctionId { get; set; }
    }
}