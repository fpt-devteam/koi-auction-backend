
namespace BiddingService.Dto.AuctionLot
{
    public class AuctionLotBidDto
    {
        public int AuctionLotId { get; set; }
        public int AuctionMethodId { get; set; }
        public int StepPercent { get; set; }
        public decimal StartPrice { get; set; }

    }
}