using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.Dto.AuctionLot
{
    public class AuctionLotBidDto
    {

        public int AuctionLotId { get; set; }
        public int? AuctionMethodId { get; set; }
        public decimal? StartPrice { get; set; }
        public int? StepPercent { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? StartTime { get; set; }
    }
}