using System.ComponentModel.DataAnnotations;
using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;
using Newtonsoft.Json.Serialization;

namespace AuctionService.Dto.Auction
{
    public class CreateAuctionDto
    {

        // [Required]
        // public int StaffId { get; set; }
        [Required]
        // [CustomValidation(typeof(AuctionHelper), nameof(AuctionHelper.ValidateFutureDate))]
        [CustomValidation(typeof(AuctionHelper), nameof(AuctionHelper.IsValid))]
        //start time must be in the future        
        public DateTime StartTime { get; set; }
        // public DateTime? EndTime { get; set; }
    }
}