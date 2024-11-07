using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Models;
using AuctionService.Dto.Lot;
using AuctionService.Helper;


namespace AuctionService.IRepository
{
    public interface ILotRepository
    {
        Task<List<Lot>> GetAllAsync(LotQueryObject lotQuery);
        Task<Lot> GetLotByIdAsync(int id);
        Task<Lot> CreateLotAsync(Lot lot);
        Task<Lot> UpdateLotAsync(int id, UpdateLotDto lotRequest);
        Task<Lot> UpdateLotStatusAsync(int id, UpdateLotStatusDto lotRequest);
        Task<Lot> DeleteLotAsync(int id);
        Task<List<LotAuctionMethodStatisticDto>> GetLotAuctionMethodStatisticAsync();
        Task<List<Lot>> GetBreederLotsStatisticsAsync();
    }
}