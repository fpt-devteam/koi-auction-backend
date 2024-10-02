using System.Runtime.CompilerServices;
using AuctionManagementService.Dto.KoiMedia;
using AuctionManagementService.Dto.LotRequestForm;
using AuctionManagementService.Models;

namespace AuctionManagementService.Mapper
{
    public static class KoiMediaMapper
    {
        public static KoiMedia ToKoiMediaFromKoiMediaDto(this KoiMediaDto koiMediaDto)
        {
            return new KoiMedia
            {
                FilePath = koiMediaDto.FilePath,
                IsPrimary = koiMediaDto.IsPrimary
            };
        }
        public static KoiMediaDto ToKoiMediaDtoFromKoiMedia(this KoiMedia koiMedia)
        {
            return new KoiMediaDto
            {
                KoiMediaId = koiMedia.KoiMediaId,
                FilePath = koiMedia.FilePath,
                IsPrimary = koiMedia.IsPrimary
            };
        }
    }
}