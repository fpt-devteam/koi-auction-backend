using AuctionService.Dto.KoiFish;
using AuctionService.Dto.KoiMedia;

namespace AuctionService.Dto.SoldLot
{
    public class SoldLotDto
    {
        public int SoldLotId { get; set; }
        public int WinnerId { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public KoiFishDto? KoiFish { get; set; }

    }
}