using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.LotStatus
{
    public class UpdateLotStatusDto
    {
        [Required]
        public string LotStatusName { get; set; }
    }
}