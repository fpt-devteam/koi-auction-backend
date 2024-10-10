using AuctionService.Dto.Auction;
using AuctionService.Models;

namespace AuctionService.Mapper
{
    public static class AuctionMapper
    {
        public static AuctionDto ToAuctionDtoFromAuction(this Auction auction)
        {
            return new AuctionDto
            {
                AuctionId = auction.AuctionId,
                AuctionName = auction.AuctionName,
                StaffId = auction.StaffId,
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                CreatedAt = auction.CreatedAt,
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