using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.KoiFish;
using AuctionManagementService.Models;

namespace AuctionManagementService.IRepository
{
    public interface IKoiFishRepository
    {
        Task<KoiFish> CreateKoiAsync(KoiFish koiFish);
        Task<KoiFish> UpdateKoiAsync(int id, UpdateKoiFishDto updateKoiDto);
    }
}