using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiddingService.Dto.BidLog
{
    public class MaxBidAmountDto
    {
        public int AuctionLotId { get; set; }
        public decimal MaxBidAmount { get; set; }
    }
}