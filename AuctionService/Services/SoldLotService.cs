using AuctionService.Dto.BidLog;
using AuctionService.Dto.SoldLot;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Models;

namespace AuctionService.Services
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

        public async Task<List<SoldLotDto>> GetAllAsync(SoldLotQueryObject queryObject)
        {
            var soldLots = await _unitOfWork.SoldLot.GetAllAsync(queryObject);
            var soldLotDto = soldLots.Select(s => s.ToSoldLotDtoFromSoldLot());
            return soldLotDto.ToList();
        }

        public async Task<SoldLotDto> GetSoldLotById(int id)
        {
            var soldLot = await _unitOfWork.SoldLot.GetSoldLotById(id);
            return soldLot.ToSoldLotDtoFromSoldLot();
        }
    }
}