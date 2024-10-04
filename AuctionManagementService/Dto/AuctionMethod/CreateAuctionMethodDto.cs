using System.ComponentModel.DataAnnotations;

namespace AuctionManagementService.Dto
{
    public class CreateAuctionMethodDto
    {
        [Required]
        public string AuctionMethodName { get; set; }
        [Required]
        public string? Description { get; set; }
    }
}