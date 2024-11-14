using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionDeposit;
using AuctionService.Dto.Wallet;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Models;

namespace AuctionService.Services
{
    public class AuctionDepositService : IAuctionDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WalletService _walletService;
        public AuctionDepositService(IUnitOfWork unitOfWork, WalletService walletService)
        {
            _unitOfWork = unitOfWork;
            _walletService = walletService;
        }

        public async Task<AuctionDeposit> CreateAuctionDepositAsync(AuctionDeposit auctionDeposit)
        {
            System.Console.WriteLine($"service: auctionlotid: {auctionDeposit.AuctionLotId}");
            var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionDeposit.AuctionLotId);
            if (auctionLot == null)
            {
                throw new Exception("Auction lot not found");
            }
            if (auctionLot.AuctionLotStatusId != (int)Enums.AuctionLotStatus.Scheduled)
            {
                throw new Exception("Auction lot is not scheduled");
            }
            if (await _unitOfWork.AuctionDeposits.GetAuctionDepositByAuctionLotIdAndUserId(auctionDeposit.UserId, auctionDeposit.AuctionLotId) != null)
            {
                throw new Exception("Auction deposit existed");
            }

            try
            {
                PaymentDto paymentDto = new PaymentDto()
                {
                    UserId = auctionDeposit.UserId,
                    SoldLotId = auctionDeposit.AuctionLotId,
                    Amount = auctionDeposit.Amount,
                    Description = $"Place deposit for auction lot {auctionDeposit.AuctionLotId}"
                };
                await _walletService.PaymentAsync(paymentDto);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var newAuctionDeposit = await _unitOfWork.AuctionDeposits.CreateAuctionDepositAsync(auctionDeposit);
            await _unitOfWork.SaveChangesAsync();
            return newAuctionDeposit;
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