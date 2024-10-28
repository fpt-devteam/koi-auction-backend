using System;
using System.Collections.Generic;

namespace AuctionService.Models;

public partial class AuctionLotJob
{
    public int Id { get; set; }

    public int? AuctionLotId { get; set; }

    public string? HangfireJobId { get; set; }

}
