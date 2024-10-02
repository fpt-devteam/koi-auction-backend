using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.Lot;
using AuctionManagementService.Dto.LotRequestForm;
using AuctionManagementService.Models;
using AuctionManagementService.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuctionManagementService.Mapper
{
    public static class LotMapper
    {
        public static Lot ToLotFromLotRequestFormDto(this LotRequestFormDto lotRequestFormDto)
        {
            return new Lot 
            {
                BreederId = lotRequestFormDto.BreederId,
                StartingPrice = lotRequestFormDto.StartingPrice,
                AuctionMethodId = lotRequestFormDto.AuctionMethodId
            };
        }

        public static LotDto ToLotDtoFromLot(this Lot lot)
        {
            return new LotDto
            {
                LotId = lot.LotId,
                StartingPrice = lot.StartingPrice,
                CreatedAt = lot.CreatedAt,
                AuctionMethod = lot.AuctionMethod.ToAuctionMethodDtoFromAuctionMethod(),
                BreederId = lot.BreederId,
                KoiFishDto = lot.KoiFish.ToKoiFishDtoFromKoiFish(),
                LotStatusDto = lot.LotStatus.ToLotStatusDtoFromLotStatus()
            };
        }
    }
}