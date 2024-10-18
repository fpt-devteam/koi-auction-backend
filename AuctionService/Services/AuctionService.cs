

using AuctionService.IRepository;
using AuctionService.IServices;

namespace AuctionService.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuctionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CheckAndStartAuction()
        {
            throw new NotImplementedException();
        }
    }
}
