using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiddingService.Dto.BidLog
{
    public class HighestBidLog
    {
        public int BidderId { get; set; }
        public decimal BidAmount { get; set; }

    }
}