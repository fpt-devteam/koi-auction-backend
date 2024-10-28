using AuctionService.Dto.SoldLot;
using AuctionService.Models;

namespace AuctionService.Mappers
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
    }
}