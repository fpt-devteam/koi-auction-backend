using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.Dto.Wallet
{
    public class PaymentDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}