using AuctionManagementService.Dto.KoiMedia;
using AuctionManagementService.Models;

namespace AuctionManagementService.IRepository
{
    public interface IKoiMediaRepository
    {
         Task<KoiMedia> CreateKoiMediaAsync(KoiMedia koiMedia);
         Task<List<KoiMedia>> DeleteKoiMediaAsync(int id);
    }
}