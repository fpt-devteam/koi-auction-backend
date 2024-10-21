
using AuctionService.IRepository;
using AuctionService.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controller
{
    [Route("api/auction-lot-statuses")]
    [ApiController]
    public class AuctionLotStatusCotnroller : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuctionLotStatusCotnroller(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuctionLotStatus()
        {
            var AuctionLotStatus = await _unitOfWork.AuctionLotStatus.GetAllAsync();
            var AuctionLotStatusDto = AuctionLotStatus.Select(s => s.ToAuctionLotStatusDtoFromAuctionLotStatus());
            return Ok(AuctionLotStatusDto);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionLotStatusById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var AuctionLotStatus = await _unitOfWork.AuctionLotStatus.GetAuctionLotStatusByIdAsync(id);
                return Ok(AuctionLotStatus.ToAuctionLotStatusDtoFromAuctionLotStatus());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}