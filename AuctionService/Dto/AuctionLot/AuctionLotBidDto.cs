using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.Dto.AuctionLot
{
    public class AuctionLotBidDto
    {
        [Required]
        public int AuctionLotId { get; set; }
        [Required]
        public int AuctionMethodId { get; set; }
        public decimal? StartPrice { get; set; }
        public int? StepPercent { get; set; }

    }
}