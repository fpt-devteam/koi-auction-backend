using System.ComponentModel.DataAnnotations;
using AuctionManagementService.Dto.AuctionLot;

namespace AuctionManagementService.Dto.Auction
{
    public class CreateAuctionDto
    {
        
        [Required]
        public int StaffId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}