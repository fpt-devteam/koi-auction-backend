using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionManagementService.Dto.KoiFish;
using AuctionManagementService.Dto.Lot;
using AuctionManagementService.Dto.LotRequestForm;
using AuctionManagementService.Helper;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using AuctionManagementService.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuctionManagementService.Controller
{
    [Route("api/lots")]
    [ApiController]
    public class LotController : ControllerBase
    {
        private readonly ILotRepository _lotRepository;
        private readonly IKoiFishRepository _koiFishRepository;
        public LotController(ILotRepository lotRepository, IKoiFishRepository koiFishRepository)
        {
            _lotRepository = lotRepository;
            _koiFishRepository = koiFishRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLot([FromQuery] LotQueryObject query)
        {
            var lots = await _lotRepository.GetAllAsync(query);
            var lotDtos = lots.Select(l => l.ToLotDtoFromLot());
            return Ok(lotDtos);
        }
        [HttpGet]
        [ActionName(nameof(GetLotById))]
        [Route("{id:int}")]
        public async Task<IActionResult> GetLotById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var lot = await _lotRepository.GetLotByIdAsync(id);
            if (lot == null)
            {
                return NotFound();
            }
            return Ok(lot.ToLotDtoFromLot());
        }


        [HttpPost]
        public async Task<IActionResult> CreateLot([FromBody] LotRequestFormDto lotRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newLot = lotRequest.ToLotFromLotRequestFormDto();
            var modelLot = await _lotRepository.CreateLotAsync(newLot);

            var newKoiFish = lotRequest.ToKoiFishFromLotRequestFormDto();
            newKoiFish.KoiFishId = modelLot.LotId;
            await _koiFishRepository.CreateKoiAsync(newKoiFish);
            return CreatedAtAction(nameof(GetLotById), new { id = newLot.LotId }, newLot.ToLotDtoFromLot());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateLot([FromRoute] int id, [FromBody] UpdateLotDto updateLotDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updateLot = await _lotRepository.UpdateLotAsync(id, updateLotDto);
            return Ok(updateLot.ToLotDtoFromLot());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteLot([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var deleteLot = await _lotRepository.DeleteLotAsync(id);
            if (deleteLot == null)
                return NotFound();
            return NoContent();
        }
    }
}