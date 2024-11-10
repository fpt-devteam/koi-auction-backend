namespace AuctionService.Dto.Dashboard
{
    public class DailyRevenueDto
    {
        public string? DayName { get; set; } // Chỉ cần tên ngày (VD: "Nov 03")
        public decimal Revenue { get; set; } // Tổng doanh thu cho ngày đó
    }
}