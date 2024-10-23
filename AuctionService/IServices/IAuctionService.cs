

namespace AuctionService.IServices
{
    public interface IAuctionService
    {
        public void ScheduleAuction(int auctionId, DateTime startTime);

        // public Task StartAuction(int auctionId, DateTime startTime);
    }
}