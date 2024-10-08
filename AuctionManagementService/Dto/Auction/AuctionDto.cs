using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Models;


namespace AuctionManagementService.Dto.Auction
{
    public class AuctionDto
    {
        public int AuctionId { get; set; }
        public int StaffId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
