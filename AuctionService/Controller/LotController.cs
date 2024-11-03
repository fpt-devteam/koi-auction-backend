using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionMethod;
using AuctionService.Dto.BreederDetail;
using AuctionService.Dto.KoiFish;
using AuctionService.Dto.Lot;
using AuctionService.Dto.LotRequestForm;
using AuctionService.Dto.LotStatus;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Models;
using AuctionService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AuctionService.Controller
{
    [Route("api/lots")]
    [ApiController]
    public class LotController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        // private readonly BreederDetailController _breederDetailController;

        private readonly BreederDetailService _breederService;
        private readonly ILotService _lotService;
        public LotController(IUnitOfWork unitOfWork, BreederDetailService service, ILotService lotService)
        {
            _unitOfWork = unitOfWork;
            _breederService = service;
            _lotService = lotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLot([FromQuery] LotQueryObject query)
        {

            var lots = await _unitOfWork.Lots.GetAllAsync(query);

            // Tạo LotDto và gán thông tin người dùng
            var tasks = lots.Select(async lot =>
            {
                var breeder = await _breederService.GetBreederByIdAsync(lot.BreederId);
                var lotDto = lot.ToLotDtoFromLot();
                lotDto.BreederDetailDto = breeder;
                return lotDto;
            }).ToList();

            // Đợi tất cả các tác vụ hoàn thành
            var lotDtos = await Task.WhenAll(tasks);

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

            var lotDto = lot.ToLotDtoFromLot();
            lotDto.BreederDetailDto = await _breederService.GetBreederByIdAsync(lot.BreederId);

            return Ok(lotDto);
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

            //add media
            var newKoiMedia = lotRequest.KoiMedia.Select(m => m.ToKoiMediaFromFormKoiMediaDto());
            if (newKoiMedia == null)
                return NotFound();
            foreach (var media in newKoiMedia)
            {
                newKoiFish.KoiMedia.Add(media);
            }

            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
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
            if (updateKoiMedia == null)
            {
                return BadRequest();
            }
            foreach (var media in updateKoiMedia)
            {
                var newMedia = media.ToKoiMediaFromFormKoiMediaDto();
                newMedia.KoiFishId = id;
                await _unitOfWork.KoiMedia.CreateKoiMediaAsync(newMedia);
            }
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return Ok(updateLot.ToLotDtoFromLot());
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateLotStatus(int id, UpdateLotStatusDto lotStatusDto)
        {
            var updateLot = await _unitOfWork.Lots.UpdateLotStatusAsync(id, lotStatusDto);
            if (updateLot == null)
                return BadRequest();
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
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
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return NoContent();
        }
        [HttpGet("auction-method-statistic")]
        public async Task<ActionResult<List<AuctionMethodDto>>> GetLotAuctionMethodStatistic()
        {
            var statistic = await _lotService.GetLotAuctionMethodStatisticAsync();
            return Ok(statistic);
        }
    }
}