using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using AuctionManagementService.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuctionManagementService.Controller
{
    [Route("auction-lots")]
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
                await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId, 
                                                new Dto.Lot.UpdateLotStatusDto {LotStatusName = "In auction"} );
                var newAuctionLot = await _unitOfWork.AuctionLots.CreateAsync(auctionLot);
                _unitOfWork.SaveChanges();
            return CreatedAtAction(nameof(GetAuctionById), new{id = newAuctionLot.AuctionLotId}, newAuctionLot);   
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult> UpdateAuctionLot([FromRoute] int id, [FromBody] UpdateAuctionLotDto auctionLotDto )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var auctionLot = await _unitOfWork.AuctionLots.UpdateAsync(id, auctionLotDto);
            _unitOfWork.SaveChanges();
            return Ok(auctionLot.ToAuctionLotDtoFromActionLot());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult> DeleteAuctionLot([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var auctionLot = await _unitOfWork.AuctionLots.DeleteAsync(id);
            if(auctionLot == null)
            {
                return NotFound();
            }
            await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId, 
                                                new Dto.Lot.UpdateLotStatusDto {LotStatusName = "Approved"} );
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}