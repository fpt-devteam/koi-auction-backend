using AuctionService.Dto.AuctionLotStatus;
using AuctionService.Models;

namespace AuctionService.Mapper
{
    public static class AuctionLotStatusMapper
    {
        public static AuctionLotStatusDto ToAuctionLotStatusDtoFromAuctionLotStatus(this AuctionLotStatus auctionLotStatus)
        {
            if (auctionLotStatus == null)
            {
                return null!;
            }
            return new AuctionLotStatusDto
            {
                AuctionStatusId = auctionLotStatus.AuctionLotStatusId,
                AuctionStatusName = auctionLotStatus.StatusName
            };
        }
    }
}