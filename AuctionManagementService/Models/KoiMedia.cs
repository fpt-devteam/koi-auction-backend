using System;
using System.Collections.Generic;

namespace AuctionManagementService.Models;

public partial class KoiMedia
{
    public int KoiMediaId { get; set; }

    public int KoiFishId { get; set; }

    public string FilePath { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual KoiFish KoiFish { get; set; } = null!;
}
