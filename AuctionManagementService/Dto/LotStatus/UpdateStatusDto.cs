using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto.LotStatus
{
    public class UpdateStatusDto
    {
        [Required]
        public string? LotStatusName { get; set; }
    }
}