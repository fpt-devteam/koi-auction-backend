using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.Auction
{
    public class UpdateAuctionDto
    {
        [Required]
        public int StaffId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
    }
}