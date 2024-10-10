using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.KoiFish;
using AuctionService.Models;

namespace AuctionService.IRepository
{
    public interface IKoiFishRepository
    {
        Task<KoiFish> CreateKoiAsync(KoiFish koiFish);
        Task<KoiFish> UpdateKoiAsync(int id, UpdateKoiFishDto updateKoiDto);
    }
}