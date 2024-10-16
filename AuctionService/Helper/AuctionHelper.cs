using System.Runtime.CompilerServices;
using AuctionManagementService.Models;
using AuctionService.Models;

namespace AuctionService.Helper
{
    public static class AuctionHelper
    {
        public static string GenerateAuctionName(Auction auction)
        {
            string staffPart = $"STF{auction.StaffId}";
            string startTime = auction.StartTime.ToString("ddHHmmss"); ;    // Ngày (2 chữ số)
            string dayPart = DateTime.UtcNow.Day.ToString("D2"); // Tháng (2 chữ số)
            return $"AUCTION-{staffPart}#{startTime}{dayPart}";
        }
    }
}