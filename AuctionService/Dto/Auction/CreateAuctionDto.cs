using System.ComponentModel.DataAnnotations;
using AuctionService.Dto.AuctionLot;

namespace AuctionService.Dto.Auction
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