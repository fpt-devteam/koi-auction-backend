using AuctionManagementService.Data;
using AuctionManagementService.Dto.KoiMedia;
using AuctionManagementService.IRepository;
using AuctionManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionManagementService.Repository
{
    public class KoiMediaRepository : IKoiMediaRepository
    {
        private readonly AuctionManagementDbContext _context;
        public KoiMediaRepository(AuctionManagementDbContext context)
        {
            _context = context;
        }
        public async Task<KoiMedia> CreateKoiMediaAsync(KoiMedia koiMedia)
        {
            await _context.KoiMedia.AddAsync(koiMedia);
            
            return koiMedia;
        }

        public async Task<List<KoiMedia>> DeleteKoiMediaAsync(int id)
        {
            var deleteKoiMedia = await _context.KoiMedia.Where(m => m.KoiFishId == id).ToListAsync();
           if (deleteKoiMedia == null)
           {
            return null;
           }
           foreach(var media in deleteKoiMedia)
           {
                _context.KoiMedia.Remove(media);
           } 
            return deleteKoiMedia;
        }
    }
}