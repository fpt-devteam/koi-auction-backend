using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionDeposit;
using AuctionService.Models;

namespace AuctionService.IRepository
{
    public interface IAuctionDepositRepository
    {
        Task<List<AuctionDeposit>> GetAuctionDepositByAuctionLotId(int auctionLotId);
        Task<AuctionDeposit> CreateAuctionDepositAsync(AuctionDeposit auctionDeposit);
        Task<AuctionDeposit> UpdateAuctionDepositStatusAsync(UpdateAuctionDepositDto updateAuctionDepositDto);
        Task<AuctionDeposit> GetAuctionDepositByAuctionLotIdAndUserId(int userId, int auctionLotId);

    }
}