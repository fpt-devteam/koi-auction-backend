using AuctionService.IRepository;
using AuctionService.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controller
{
    [Route("api/auction-statuses")]
    [ApiController]

    public class AuctionStatusController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuctionStatusController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAuctionStatus()
        {
            var AuctionStatus = await _unitOfWork.AuctionStatus.GetAllAsync();
            var AuctionStatusDto = AuctionStatus.Select(s => s.ToAuctionStatusDtoFromAuctionStatus());
            return Ok(AuctionStatusDto);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionStatusById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var AuctionStatus = await _unitOfWork.AuctionStatus.GetAuctionStatusByIdAsync(id);
                return Ok(AuctionStatus.ToAuctionStatusDtoFromAuctionStatus());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }


        }
    }
}