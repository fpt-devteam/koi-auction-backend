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
        private readonly IUnitOfWork _unitOfWork;
        public LotController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLot([FromQuery] LotQueryObject query)
        {
            var lots = await _unitOfWork.Lots.GetAllAsync(query);
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
            var lot = await _unitOfWork.Lots.GetLotByIdAsync(id);
            if (lot == null)
            {
                return NotFound();
            }
            return Ok(lot.ToLotDtoFromLot());
        }


        [HttpPost]
        public async Task<IActionResult> CreateLot([FromBody] CreateLotRequestFormDto lotRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //add lot
            var newLot = lotRequest.ToLotFromCreateLotRequestFormDto();
            newLot.Sku = LotHelper.GenerateSku(newLot);
            var modelLot = await _unitOfWork.Lots.CreateLotAsync(newLot);

            //add koifish
            var newKoiFish = lotRequest.ToKoiFishFromCreateLotRequestFormDto();
            modelLot.KoiFish = newKoiFish;
            //newKoiFish.KoiFishId = modelLot.LotId;
            //await _unitOfWork.KoiFishes.CreateKoiAsync(newKoiFish);

            //add media
            var newKoiMedia = lotRequest.KoiMedia.Select(m => m.ToKoiMediaFromFormKoiMediaDto());
            if (newKoiMedia == null)
                return NotFound();
            foreach (var media in newKoiMedia)
            {
                newKoiFish.KoiMedia.Add(media);
                //media.KoiFishId = newKoiFish.KoiFishId;
                //await _unitOfWork.KoiMedia.CreateKoiMediaAsync(media);
            }

            _unitOfWork.SaveChanges();
            return CreatedAtAction(nameof(GetLotById), new { id = newLot.LotId }, newLot.ToLotDtoFromLot());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateLot([FromRoute] int id, [FromBody] UpdateLotRequestFormDto lotRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //update lot
            var updateLotDto = lotRequest.ToUpdateLotDtoFromLotRequestFormDto();
            var updateLot = await _unitOfWork.Lots.UpdateLotAsync(id, updateLotDto);

            //update koifish
            var updateKoiFishDto = lotRequest.ToUpdateKoiFishDtoFromUpdateLotRequestFormDto();
           var updateKoiFish = await _unitOfWork.KoiFishes.UpdateKoiAsync(id, updateKoiFishDto);

            //update media
            await _unitOfWork.KoiMedia.DeleteKoiMediaAsync(id);
            var updateKoiMedia = lotRequest.KoiMedia;
            if(updateKoiMedia == null)
            {
                return BadRequest();
            }
            foreach (var media in updateKoiMedia)
            {
                var newMedia = media.ToKoiMediaFromFormKoiMediaDto();
                newMedia.KoiFishId = id;
                await _unitOfWork.KoiMedia.CreateKoiMediaAsync(newMedia);
            }
            _unitOfWork.SaveChanges();
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
            var deleteLot = await _unitOfWork.Lots.DeleteLotAsync(id);
            if (deleteLot == null)
                return NotFound();
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}