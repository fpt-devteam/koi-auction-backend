namespace AuctionService
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Details { get; set; }
    }
}