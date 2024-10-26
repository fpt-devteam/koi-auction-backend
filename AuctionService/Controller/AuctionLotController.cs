using AuctionService.Dto.AuctionLot;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controller
{
    [Route("api/auction-lots")]
    [ApiController]
    public class AuctionLotController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BreederDetailController _breederDetailController;

        private readonly IAuctionLotService _auctionLotService;
        public AuctionLotController(IUnitOfWork unitOfWork, BreederDetailController breederDetailController, IAuctionLotService auctionLotService)
        {
            _unitOfWork = unitOfWork;
            _breederDetailController = breederDetailController;
            _auctionLotService = auctionLotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuctionLot([FromQuery] AuctionLotQueryObject query)
        {
            var auctionLots = await _unitOfWork.AuctionLots.GetAllAsync(query);
            var tasks = auctionLots.Select(async auctionLot =>
            {
                var auctionLotDto = auctionLot.ToAuctionLotDtoFromAuctionLot();
                auctionLotDto!.LotDto!.BreederDetailDto = await _breederDetailController.GetBreederByIdAsync(auctionLotDto.LotDto.BreederId);
                return auctionLotDto;
            }).ToList();

            var auctionLotDtos = await Task.WhenAll(tasks);
            return Ok(auctionLotDtos);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionLotById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(id);
            if (auctionLot == null)
                return NotFound();
            var auctionLotDto = auctionLot.ToAuctionLotDtoFromAuctionLot();
            auctionLotDto!.LotDto!.BreederDetailDto = await _breederDetailController.GetBreederByIdAsync(auctionLotDto.LotDto.BreederId);
            return Ok(auctionLotDto);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAuctionLot([FromBody] CreateAuctionLotDto auctionLotDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var auctionLot = auctionLotDto.ToAuctionLotFromCreateAuctionLotDto();
                await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId,
                                                new Dto.Lot.UpdateLotStatusDto { LotStatusName = "In auction" });
                var newAuctionLot = await _unitOfWork.AuctionLots.CreateAsync(auctionLot);
                if (!await _unitOfWork.SaveChangesAsync())
                {
                    return BadRequest("An error occurred while saving the data");
                }
                return CreatedAtAction(nameof(GetAuctionLotById), new { id = newAuctionLot.AuctionLotId }, newAuctionLot);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // call from auction service to end auction lot
        // [HttpPut]
        // [Route("endAuctionLot/{auctionLotId:int}")]
        // public async Task<ActionResult> EndAuctionLot([FromRoute] int auctionLotId, [FromBody] EndAuctionLotDto endAuctionLotDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(auctionLotId);
        //     if (auctionLot == null)
        //     {
        //         return NotFound();
        //     }
        //     await _auctionLotService.EndAuctionLot(auctionLot.AuctionLotId, endAuctionLotDto.EndTime);
        //     return Ok(auctionLot.ToAuctionLotDtoFromAuctionLot());
        // }
        [HttpPut]
        [Route("updateEndTime/{auctionLotId:int}")]
        public async Task<ActionResult> UpdateEndTimeAuctionLot([FromRoute] int auctionLotId, [FromBody] UpdateEndTimeAuctionLotDto updateEndTimeAuctionLotDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _auctionLotService.UpdateEndTimeAuctionLot(auctionLotId, updateEndTimeAuctionLotDto.EndTime);
            return Ok();
        }

        // [HttpPost]
        // [Route("test-start-auction-lot")]
        // public async Task<ActionResult> TestStartAuctionLot([FromBody] TestStartAuctionLotDto testStartAuctionLotDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //     try
        //     {
        //         await _auctionLotService.StartAuctionLot(testStartAuctionLotDto.AuctionLotId);
        //         return Ok(new { message = "Auction lot is starting!" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }

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
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return CreatedAtAction(nameof(GetAuctionLotById), new { id = auctionLots.First().AuctionLotId }, auctionLots);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult> UpdateAuctionLot([FromRoute] int id, [FromBody] UpdateAuctionLotDto auctionLotDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(id);
            if (auctionLot == null)
                return NotFound();
            _unitOfWork.AuctionLots.Update(auctionLot, auctionLotDto);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return Ok(auctionLot.ToAuctionLotDtoFromAuctionLot());
        }

        [HttpPatch]
        [Route("{id:int}")]
        public async Task<ActionResult> PatchAuctionLot([FromRoute] int id, [FromBody] JsonPatchDocument<UpdateAuctionLotDto> patchDoc)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var auctionLot = await _unitOfWork.AuctionLots.GetAuctionLotById(id);
            if (auctionLot == null)
            {
                return NotFound();
            }
            var updateAuctionLotDto = auctionLot.ToUpdateAuctionLotDtoFromAuctionLot();
            patchDoc.ApplyTo(updateAuctionLotDto, ModelState);
            if (!TryValidateModel(updateAuctionLotDto))
            {
                return ValidationProblem(ModelState);
            }
            _unitOfWork.AuctionLots.Update(auctionLot, updateAuctionLotDto);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return Ok(auctionLot.ToAuctionLotDtoFromAuctionLot());
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
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return NoContent();
        }

        [HttpDelete("listAuctionLot")]
        public async Task<ActionResult> DeleteListAuctionLot([FromBody] List<int> ids)
        {
            if (ids == null)
            {
                return BadRequest("List is empty");
            }

            var deletedAuctionLots = await _unitOfWork.AuctionLots.DeleteListAsync(ids);

            if (deletedAuctionLots == null)
            {
                return NotFound("ids not existed");
            }

            foreach (var auctionLot in deletedAuctionLots)
            {
                await _unitOfWork.Lots.UpdateLotStatusAsync(auctionLot.AuctionLotId,
                                            new Dto.Lot.UpdateLotStatusDto { LotStatusName = "Approved" });
            }

            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }

            return Ok(deletedAuctionLots);
        }
    }

}