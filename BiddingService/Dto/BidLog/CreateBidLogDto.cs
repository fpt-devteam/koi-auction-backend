using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiddingService.Dto.BidLog
{
    public class CreateBidLogDto
    {
        public int BidderId { get; set; }

        public int AuctionLotId { get; set; }

        public int BidAmount { get; set; }

    }
}