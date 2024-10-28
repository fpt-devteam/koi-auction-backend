using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.IRepositories
{
    public interface IUnitOfWork
    {
        IBidLogRepository BidLog { get; }
        Task<bool> SaveChangesAsync();
    }
}