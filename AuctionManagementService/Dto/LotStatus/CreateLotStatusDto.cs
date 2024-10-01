using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.LotStatus
{
    public class CreateLotStatusDto
    {
        [Required]
        public string LotStatusName { get; set; }
    }
}