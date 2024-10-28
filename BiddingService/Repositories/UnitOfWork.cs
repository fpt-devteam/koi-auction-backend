using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.IRepositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BiddingDbContext _context;
        public UnitOfWork(BiddingDbContext context)
        {
            _context = context;
            BidLog = new BidLogRepository(_context);
            SoldLot = new SoldLotRepository(_context);
        }
        public IBidLogRepository BidLog { get; private set; }

        public ISoldLotRepository SoldLot { get; private set; }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547)) // 547 là mã lỗi khóa ngoại của SQL Server
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}