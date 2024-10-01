using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionManagementService.Dto.KoiFish
{
    public class KoiFishDto
    {
        public string Variety { get; set; } = null!;

        public bool Sex { get; set; }

        public decimal SizeCm { get; set; }

        public int YearOfBirth { get; set; }

        public decimal WeightKg { get; set; }
    }
}