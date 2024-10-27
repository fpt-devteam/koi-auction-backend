using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Models;

namespace BiddingService.IServices
{
    public interface IBidLogService
    {
        Task<List<BidLog>> GetAllBidLog();
        Task<BidLog> GetBidLogById(int id);
        //Task<BidLog> CreateBidLog(BidLog bidLog);

    }
}