using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.Lot
{
    public class UpdateLotStatusDto
    {
        [Required]
        public string? LotStatusName { get; set; }
    }
}