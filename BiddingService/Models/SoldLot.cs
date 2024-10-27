using System;
using System.Collections.Generic;

namespace BiddingService.Models;

public partial class SoldLot
{
    public int SoldLotId { get; set; }

    public int WinnerId { get; set; }

    public decimal FinalPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
