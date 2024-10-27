using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Helper;
using BiddingService.Models;

namespace BiddingService.IServices
{
    public interface IBidLogService
    {
        Task<List<BidLog>> GetAllBidLog(BidLogQueryObject queryObject);
        Task<BidLog> GetBidLogById(int id);
        //Task<BidLog> CreateBidLog(BidLog bidLog);

    }
}