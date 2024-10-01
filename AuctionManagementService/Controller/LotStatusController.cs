using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.LotStatus;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace AuctionManagementService.Controller
{
    [Route("api/lot-statuses")]
    [ApiController]
    public class LotStatusController : ControllerBase
    {
        private readonly ILotStatusRepository _repo;
        public LotStatusController(ILotStatusRepository repo)
        {
            _repo = repo;            
        }
        [HttpGet]
        public async Task<IActionResult> GetAllLotStatus()
        {
            var lotStatus = await _repo.GetAllAsync();
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
            var lotStatus = await _repo.GetLotStatusByIdAsync(id);
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
            var newLotStatus = await _repo.CreateLotStatusAsync(lotStatus);
            return CreatedAtAction(nameof(GetLotStatusById), new{id = newLotStatus.LotStatusId}, newLotStatus);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateLotStatus([FromRoute] int id, [FromBody] UpdateLotStatusDto lotStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updateLotStatus = await _repo.UpdateLotStatusAsync(id, lotStatusDto);
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
            var deleteLotStatus = await _repo.DeleteLotStatusAsync(id);
            if(deleteLotStatus == null)
            {
                NotFound();
            }
            return NoContent();
        }
    }
}