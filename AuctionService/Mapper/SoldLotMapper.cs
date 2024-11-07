using AuctionService.Dto.SoldLot;
using AuctionService.Models;

namespace AuctionService.Mapper
{
    public static class SoldLotMapper
    {
        public static SoldLot ToSoldLotDtoFromCreateSoldLotDto(this CreateSoldLotDto createSoldLotDto)
        {
            return new SoldLot
            {
                SoldLotId = createSoldLotDto.SoldLotId,
                WinnerId = createSoldLotDto.WinnerId,
                FinalPrice = createSoldLotDto.FinalPrice
            };
        }

        public static SoldLotDto ToSoldLotDtoFromSoldLot(this SoldLot soldLot)
        {
            return new SoldLotDto
            {
                SoldLotId = soldLot.SoldLotId,
                WinnerId = soldLot.WinnerId,
                FinalPrice = soldLot.FinalPrice,
                CreatedAt = soldLot.CreatedAt,
                UpdatedAt = soldLot.UpdatedAt,
                KoiFish = soldLot.SoldLotNavigation.AuctionLotNavigation.KoiFish.ToKoiFishDtoFromKoiFish()
            };
        }
    }
}