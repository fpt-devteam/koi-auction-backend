using BiddingService.Dto.BidLog;
using BiddingService.Dto.SoldLot;
using BiddingService.IRepositories;
using BiddingService.IServices;
using BiddingService.Mappers;
using BiddingService.Models;

namespace BiddingService.Services
{
    public class SoldLotService : ISoldLotService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SoldLotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<SoldLot> CreateSoldLot(CreateSoldLotDto soldLotDto)
        {
            var soldLot = soldLotDto.ToSoldLotDtoFromCreateSoldLotDto();
            if (soldLot == null)
                throw new ArgumentNullException("CreateSoldLotDto cannot be changed to SoldLot");
            await _unitOfWork.SoldLot.CreateSoldLot(soldLot);
            await _unitOfWork.SaveChangesAsync();
            return soldLot;
        }

        public async Task<List<SoldLot>> GetAllAsync()
        {
            return await _unitOfWork.SoldLot.GetAllAsync();
        }

        public async Task<SoldLot> GetSoldLotById(int id)
        {
            return await _unitOfWork.SoldLot.GetSoldLotById(id);
        }
    }
}