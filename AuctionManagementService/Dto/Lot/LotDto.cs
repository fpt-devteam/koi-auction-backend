using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.AuctionMethod;
using AuctionManagementService.Dto.KoiFish;
using AuctionManagementService.Dto.LotStatus;

namespace AuctionManagementService.Dto.Lot
{
    public class LotDto
    {
        public int LotId { get; set; }
        public decimal StartingPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public AuctionMethodDto? AuctionMethod{ get; set; }
        public int BreederId { get; set; }
        public KoiFishDto? KoiFishDto { get; set; }
        public LotStatusDto? LotStatusDto { get; set; }

    }
}