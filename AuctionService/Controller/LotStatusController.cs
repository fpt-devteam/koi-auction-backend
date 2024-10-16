using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.LotStatus;
using AuctionService.IRepository;
using AuctionService.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controller
{
    [Route("api/lot-statuses")]
    [ApiController]
    public class LotStatusController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public LotStatusController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllLotStatus()
        {
            var lotStatus = await _unitOfWork.LotStatuses.GetAllAsync();
            var lotStatusDto = lotStatus.Select(l => l.ToLotStatusDtoFromLotStatus());
            return Ok(lotStatusDto);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetLotStatusById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var lotStatus = await _unitOfWork.LotStatuses.GetLotStatusByIdAsync(id);
            return Ok(lotStatus.ToLotStatusDtoFromLotStatus());
        }

        [HttpPost]
        public async Task<IActionResult> CreateLotStatus([FromBody] CreateLotStatusDto lotStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var lotStatus = lotStatusDto.ToLotStatusFromCreateLotStatusDto();
            var newLotStatus = await _unitOfWork.LotStatuses.CreateLotStatusAsync(lotStatus);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return CreatedAtAction(nameof(GetLotStatusById), new { id = newLotStatus.LotStatusId }, newLotStatus);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateLotStatus([FromRoute] int id, [FromBody] UpdateStatusDto lotStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updateLotStatus = await _unitOfWork.LotStatuses.UpdateLotStatusAsync(id, lotStatusDto);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return Ok(updateLotStatus);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteLotStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var deleteLotStatus = await _unitOfWork.LotStatuses.DeleteLotStatusAsync(id);
            if (deleteLotStatus == null)
            {
                NotFound();
            }
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return NoContent();
        }
    }
}