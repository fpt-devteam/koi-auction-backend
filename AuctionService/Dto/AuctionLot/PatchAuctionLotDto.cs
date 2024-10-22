using System.ComponentModel.DataAnnotations;
using AuctionService.Dto.Lot;

namespace AuctionService.Dto.AuctionLot
{
    public class PatchAuctionLotDto
    {
        public TimeOnly? Duration { get; set; }
        public int? OrderInAuction { get; set; }

        public int? StepPercent { get; set; }
        public int? AuctionLotId { get; set; }
        public int? AuctionId { get; set; }

    }
}