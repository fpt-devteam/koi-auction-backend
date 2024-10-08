using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AuctionManagementService.Dto.BreederDetail;
using AuctionManagementService.Dto.KoiFish;
using AuctionManagementService.Dto.Lot;
using AuctionManagementService.Dto.LotRequestForm;
using AuctionManagementService.Dto.LotStatus;
using AuctionManagementService.Helper;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using AuctionManagementService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AuctionManagementService.Controller
{
    [Route("api/lots")]
    [ApiController]
    public class LotController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        public LotController(IUnitOfWork unitOfWork, HttpClient httpClient, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
            _cache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLot([FromQuery] LotQueryObject query)
        {

            var lots = await _unitOfWork.Lots.GetAllAsync(query);
            var lotDtos = new List<LotDto>();
            foreach (var lot in lots)
            {
                // Kiểm tra xem UserId đã có trong cache chưa
                if (!_cache.TryGetValue(lot.BreederId, out BreederDetailDto? breeder))
                {
                    // Nếu không có trong cache thì gọi API
                    var userResponse = await _httpClient.GetAsync($"https://67035c76bd7c8c1ccd412a4e.mockapi.io/api/profiles/{lot.BreederId}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userContent = await userResponse.Content.ReadAsStringAsync();
                        breeder = JsonSerializer.Deserialize<BreederDetailDto>(userContent);

                        // Lưu thông tin vào cache với TTL là 10 phút
                        _cache.Set(lot.BreederId, breeder, TimeSpan.FromMinutes(1));
                    }
                }

                // Tạo LotDto và gán thông tin người dùng
                var lotDto = lot.ToLotDtoFromLot();
                lotDto.breederDetailDto = breeder;
                lotDtos.Add(lotDto);
            }
            return Ok(lotDtos);
        }
        // public async Task<IActionResult> GetAllLot([FromQuery] LotQueryObject query)
        // {
        //     // Lấy danh sách Lot
        //     var lots = await _unitOfWork.Lots.GetAllAsync(query);

        //     // Tạo danh sách các BreederId chưa có trong cache
        //     var breederIds = lots
        //         .Where(l => !_cache.TryGetValue(l.BreederId, out BreederDetailDto? _)) // Chỉ lấy các BreederId chưa có trong cache
        //         .Select(l => l.BreederId)
        //         .Distinct()
        //         .ToList();

        //     // Nếu có BreederId chưa có trong cache thì gọi batch API để lấy thông tin
        //     if (breederIds.Any())
        //     {
        //         var userResponse = await _httpClient.GetAsync($"https://67035c76bd7c8c1ccd412a4e.mockapi.io/api/profiles?ids={string.Join(",", breederIds)}");
        //         if (userResponse.IsSuccessStatusCode)
        //         {
        //             var userContent = await userResponse.Content.ReadAsStringAsync();
        //             var breeders = JsonSerializer.Deserialize<List<BreederDetailDto>>(userContent);

        //             // Lưu tất cả BreederDetailDto vào cache
        //             foreach (var breeder in breeders)
        //             {
        //                 _cache.Set(breeder.FarmName!, breeder, TimeSpan.FromMinutes(1));
        //             }
        //         }
        //     }

        //     // Tạo danh sách LotDto và lấy thông tin từ cache
        //     var lotDtos = lots.Select(lot =>
        //     {
        //         // Lấy thông tin breeder từ cache
        //         _cache.TryGetValue(lot.BreederId, out BreederDetailDto? breeder);

        //         // Tạo LotDto và gán thông tin người dùng
        //         var lotDto = lot.ToLotDtoFromLot();
        //         lotDto.breederDetailDto = breeder;

        //         return lotDto;
        //     }).ToList();

        //     return Ok(lotDtos);
        // }

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
            _unitOfWork.SaveChanges();
            return Ok(updateLot.ToLotDtoFromLot());
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateLotStatus(int id, UpdateLotStatusDto lotStatusDto)
        {
            var updateLot = await _unitOfWork.Lots.UpdateLotStatusAsync(id, lotStatusDto);
            if (updateLot == null)
                return BadRequest();
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