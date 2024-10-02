namespace AuctionManagementService.Dto.KoiMedia
{
    public class KoiMediaDto
    {
        public int KoiMediaId { get; set; }
        public string FilePath { get; set; } = null!;

        public bool IsPrimary { get; set; }
    }
}