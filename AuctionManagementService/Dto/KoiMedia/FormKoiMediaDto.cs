using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.KoiMedia
{
    public class FormKoiMediaDto
    {
        [Required]
        public string FilePath { get; set; } = null!;
        
    }
}