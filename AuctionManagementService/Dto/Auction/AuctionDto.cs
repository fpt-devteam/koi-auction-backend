using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Models;


namespace AuctionManagementService.Dto.Auction
{
    public class AuctionDto
    {
        public int StaffId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public List<AuctionLotDto?> AuctionLots { get; set; }

    }
}
