using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.KoiFish;
using AuctionManagementService.Dto.LotRequestForm;
using AuctionManagementService.Models;

namespace AuctionManagementService.Mapper
{
    public static class KoiFishMapper
    {
        public static KoiFish ToKoiFishFromLotRequestFormDto(this LotRequestFormDto lotRequestFormDto)
        {
            return new KoiFish
            {
                Variety = lotRequestFormDto.Variety,
                Sex = lotRequestFormDto.Sex,
                SizeCm = lotRequestFormDto.SizeCm,
                YearOfBirth = lotRequestFormDto.YearOfBirth,
                WeightKg = lotRequestFormDto.WeightKg
            };
        }


        public static KoiFishDto ToKoiFishDtoFromKoiFish(this KoiFish koiFish)
        {
            return new KoiFishDto
            {
                Variety = koiFish.Variety,
                Sex = koiFish.Sex,
                SizeCm = koiFish.SizeCm,
                YearOfBirth = koiFish.YearOfBirth,
                WeightKg = koiFish.WeightKg,
                KoiMedia = koiFish.KoiMedia.Select(m => m.ToKoiMediaDtoFromKoiMedia()).ToList()
            };
        }
    }
}