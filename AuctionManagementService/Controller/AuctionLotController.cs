using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using AuctionManagementService.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuctionManagementService.Controller
{
    [Route("api/auction-lots")]
    [ApiController]
    public class AuctionLotController:ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuctionLotController(IUnitOfWork unitOfWork)
        {
             _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAuctionLot()
        {
            var auctionLots = await _unitOfWork.AuctionLots.GetAllAsync();
            var auctionLotDtos = auctionLots.Select(a => a.ToAuctionLotDtoFromActionLot());
            return Ok(auctionLotDtos);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(id);
            if (auctionLot == null)
                return NotFound();
            return Ok(auctionLot.ToAuctionLotDtoFromActionLot());
        }

        [HttpPost]
        public async Task<ActionResult> CreateAuctionLot([FromBody] CreateAuctionLotDto auctionLotDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                var auctionLot = auctionLotDto.ToAuctionLotFromCreateAuctionLotDto();
                var newAuctionLot = await _unitOfWork.AuctionLots.CreateAsync(auctionLot);
                _unitOfWork.SaveChanges();
            return CreatedAtAction(nameof(GetAuctionById), new{id = newAuctionLot.AuctionLotId}, newAuctionLot);   
        }
    }
}