using AuctionManagementService.Dto.Auction;
using AuctionManagementService.Models;

namespace AuctionManagementService.Mapper
{
    public static class AuctionMapper
    {
        public static AuctionDto ToAuctionDtoFromAuction(this Auction auction)
        {
            return new AuctionDto
            {
                StaffId = auction.StaffId,
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                CreatedAt = auction.CreatedAt,
                AuctionLots = auction.AuctionLots.Select(a => a.ToAuctionLotDtoFromActionLot()).ToList()
            };
        }

        public static Auction ToAuctionFromCreateAuctionDto(this CreateAuctionDto createAuction)
        {
            return new Auction
            {
                StaffId = createAuction.StaffId,
                StartTime = createAuction.StartTime,
                EndTime = createAuction.EndTime,
            };
        }
    }
}