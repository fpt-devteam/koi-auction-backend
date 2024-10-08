using AuctionManagementService.Dto.AuctionLot;
using AuctionManagementService.Helper;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using AuctionManagementService.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuctionManagementService.Controller
{
    [Route("api/auction-lots")]
    [ApiController]
    public class AuctionLotController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BreederDetailController _breederDetailController;
        public AuctionLotController(IUnitOfWork unitOfWork, BreederDetailController breederDetailController)
        {
            _unitOfWork = unitOfWork;
            _breederDetailController = breederDetailController;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuctionLot([FromQuery] AuctionLotQueryObject query)
        {
            var auctionLots = await _unitOfWork.AuctionLots.GetAllAsync(query);
            var tasks = auctionLots.Select(async auctionLot =>
            {
                var auctionLotDto = auctionLot.ToAuctionLotDtoFromActionLot();
                auctionLotDto!.LotDto!.BreederDetailDto = await _breederDetailController.GetBreederByIdAsync(auctionLotDto.LotDto.BreederId);
                return auctionLotDto;
            }).ToList();

            var auctionLotDtos = await Task.WhenAll(tasks);
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
            var auctionLotDto = auctionLot.ToAuctionLotDtoFromActionLot();
            auctionLotDto!.LotDto!.BreederDetailDto = await _breederDetailController.GetBreederByIdAsync(auctionLotDto.LotDto.BreederId);
            return Ok(auctionLotDto);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAuctionLot([FromBody] CreateAuctionLotDto auctionLotDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auctionLot = auctionLotDto.ToAuctionLotFromCreateAuctionLotDto();
            await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId,
                                            new Dto.Lot.UpdateLotStatusDto { LotStatusName = "In auction" });
            var newAuctionLot = await _unitOfWork.AuctionLots.CreateAsync(auctionLot);
            _unitOfWork.SaveChanges();
            return CreatedAtAction(nameof(GetAuctionById), new { id = newAuctionLot.AuctionLotId }, newAuctionLot);
        }
        [HttpPost("listAuctionLot")]
        public async Task<ActionResult> CreateListAuctionLot([FromBody] List<CreateAuctionLotDto> listAuctionLotDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var auctionLots = listAuctionLotDto.Select(dto => dto.ToAuctionLotFromCreateAuctionLotDto()).ToList();

            foreach (var auctionLot in auctionLots)
            {
                await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId, new Dto.Lot.UpdateLotStatusDto
                {
                    LotStatusName = "In auction"
                });
            }

            await _unitOfWork.AuctionLots.CreateListAsync(auctionLots);
            _unitOfWork.SaveChanges();

            return CreatedAtAction(nameof(GetAuctionById), new { id = auctionLots.First().AuctionLotId }, auctionLots);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult> UpdateAuctionLot([FromRoute] int id, [FromBody] UpdateAuctionLotDto auctionLotDto)
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
            if (auctionLot == null)
            {
                return NotFound();
            }
            await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId,
                                                new Dto.Lot.UpdateLotStatusDto { LotStatusName = "Approved" });
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}