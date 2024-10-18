using System.ComponentModel.DataAnnotations;
using AuctionService.Dto.AuctionStatus;

namespace AuctionService.Dto.Auction
{
    public class UpdateAuctionDto
    {

        [Required]
        public int StaffId { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public AuctionStatusDto? AuctionStatus { get; set; }

    }
}