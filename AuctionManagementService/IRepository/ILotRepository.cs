using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.Lot;
using AuctionManagementService.Helper;
using AuctionManagementService.Models;


namespace AuctionManagementService.IRepository
{
    public interface ILotRepository
    {
        Task<List<Lot>> GetAllAsync(LotQueryObject lotQuery);
        Task<Lot> GetLotByIdAsync(int id);
        Task<Lot> CreateLotAsync(Lot lot);
        Task<Lot> UpdateLotAsync(int id, UpdateLotDto lotRequest);
        Task<Lot> DeleteLotAsync(int id);
    }
}