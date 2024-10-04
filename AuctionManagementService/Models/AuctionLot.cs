using System;
using System.Collections.Generic;

namespace AuctionManagementService.Models;

public partial class AuctionLot
{
    public int AuctionLotId { get; set; }

    public int AuctionId { get; set; }

    public TimeOnly Duration { get; set; }

    public int OrderInAuction { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int StepPercent { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual Lot AuctionLotNavigation { get; set; } = null!;
}
