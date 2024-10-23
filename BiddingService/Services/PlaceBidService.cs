using System.Collections.Concurrent;
using BiddingService.Dto.BidLog;
using BiddingService.IRepositories;


namespace BiddingService.Services
{
    public class PlaceBidService
    {
        private ConcurrentQueue<CreateBidLogDto> _bidQueue;
        private readonly IUnitOfWork _unitOfWork;
        private int _highestBid;

        private AuctionLotDto _auctionLotDto;
        public PlaceBidService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _bidQueue = new ConcurrentQueue<CreateBidLogDto>();
        }
        public void SetUp(AuctionLotDto auctionLotDto)
        {
            _auctionLotDto = auctionLotDto;
        }
    }
}
