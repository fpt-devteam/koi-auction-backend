using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Models;

namespace AuctionManagementService.Mapper
{
    public static class AuctionLotMapper
    {
        public static AuctionLotDto ToAuctionLotDtoFromActionLot(this AuctionLot auctionLot)
        {
            if (auctionLot == null)
            {
                return null;
            }
            
            return new AuctionLotDto
            {
                Duration = auctionLot.Duration,
                OrderInAuction = auctionLot.OrderInAuction,
                CreatedAt = auctionLot.CreatedAt,
                StepPercent = auctionLot.StepPercent,
                EndTime = auctionLot.EndTime,
                LotDto = auctionLot.AuctionLotNavigation.ToLotDtoFromLot()
            };
        }

        public static AuctionLot ToAuctionLotFromCreateAuctionLotDto(this CreateAuctionLotDto auctionLotDto)
        {
            return new AuctionLot
            {
                Duration = auctionLotDto.Duration,
                OrderInAuction = auctionLotDto.OrderInAuction,
                StepPercent = auctionLotDto.StepPercent,
                AuctionLotId = auctionLotDto.AuctionLotId,
                AuctionId = auctionLotDto.AuctionId
            };
        }
    }
}