using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionDeposit;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Models;

namespace AuctionService.Services
{
    public class AuctionDepositService : IAuctionDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuctionDepositService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<AuctionDeposit> CreateAuctionDepositAsync(AuctionDeposit auctionDeposit)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AuctionDeposit>> GetAuctionDepositByAuctionLotId(int auctionLotId)
        {
            var list = await _unitOfWork.AuctionDeposits.GetAuctionDepositByAuctionLotId(auctionLotId);
            return list;
        }

        public async Task<AuctionDeposit> GetAuctionDepositByAuctionLotIdAndUserId(int userId, int auctionLotId)
        {
            var auctionDeposit = await _unitOfWork.AuctionDeposits.GetAuctionDepositByAuctionLotIdAndUserId(userId, auctionLotId);
            return auctionDeposit;
        }

        public async Task<AuctionDeposit> UpdateAuctionDepositStatusAsync(UpdateAuctionDepositDto updateAuctionDepositDto)
        {
            var auctionDeposit = await _unitOfWork.AuctionDeposits.UpdateAuctionDepositStatusAsync(updateAuctionDepositDto);
            await _unitOfWork.SaveChangesAsync();
            return auctionDeposit;
        }
    }
}