using AuctionService.Dto.BidLog;
using AuctionService.Dto.SoldLot;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuctionService.Services
{
    public class SoldLotService : ISoldLotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BreederDetailService _breederService;
        private readonly UserSevice _userService;

        public SoldLotService(IUnitOfWork unitOfWork, BreederDetailService breederService, UserSevice userService)
        {
            _unitOfWork = unitOfWork;
            _breederService = breederService;
            _userService = userService;
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
            var soldLotDtos = new List<SoldLotDto>();

            // Sử dụng vòng lặp tuần tự để tránh truy cập đồng thời vào DbContext
            foreach (var s in soldLots)
            {
                var breeder = await _breederService.GetBreederByIdAsync(s.BreederId);
                var winner = await _userService.GetuserByIdAsync(s.WinnerId);
                var deposit = await _unitOfWork.AuctionDeposits.GetAuctionDepositByAuctionLotIdAndUserId(s.WinnerId, s.SoldLotId);
                var depositDto = deposit?.ToAuctionDepositDto();

                var soldLotDto = s.ToSoldLotDtoFromSoldLot();
                soldLotDto.BreederDetailDto = breeder;
                soldLotDto.WinnerDto = winner;
                soldLotDto.AuctionDepositDto = depositDto;

                soldLotDtos.Add(soldLotDto);
            }

            return soldLotDtos;
        }


        public async Task<SoldLotDto> GetSoldLotById(int id)
        {
            // Lấy thông tin SoldLot theo ID
            var soldLot = await _unitOfWork.SoldLot.GetSoldLotById(id);

            // Lấy chi tiết Breeder dựa trên BreederId
            var breeder = await _breederService.GetBreederByIdAsync(soldLot.BreederId);

            // Lấy chi tiết Winner dựa trên WinnerId
            var winner = await _userService.GetuserByIdAsync(soldLot.WinnerId);

            // Lấy thông tin AuctionDeposit dựa trên WinnerId và SoldLotId
            var deposit = await _unitOfWork.AuctionDeposits.GetAuctionDepositByAuctionLotIdAndUserId(soldLot.WinnerId, soldLot.SoldLotId);
            var depositDto = deposit?.ToAuctionDepositDto();

            // Chuyển đổi SoldLot sang SoldLotDto
            var soldLotDto = soldLot.ToSoldLotDtoFromSoldLot();

            // Gán các thông tin bổ sung vào SoldLotDto
            soldLotDto.BreederDetailDto = breeder;
            soldLotDto.WinnerDto = winner;
            soldLotDto.AuctionDepositDto = depositDto;

            return soldLotDto;
        }
    }
}