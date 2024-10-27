using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Dto.BidLog;
using BiddingService.Helper;
using BiddingService.Models;

namespace BiddingService.IRepositories
{
    public interface IBidLogRepository
    {
        Task<List<BidLog>> GetAllAsync(BidLogQueryObject queryObject);
        Task<BidLog> GetByIdAsync(int id);
        Task<BidLog> CreateAsync(BidLog bidLog);
    }

}