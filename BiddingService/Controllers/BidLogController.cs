using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Dto.BidLog;
using BiddingService.IServices;
using BiddingService.Mappers;
using BiddingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BiddingService.Controllers
{
    [Route("api/bid-log")]
    [ApiController]
    public class BidLogController : ControllerBase
    {
        private readonly IBidLogService _service;


        // private readonly PlaceBidService _placeBidService;
        public BidLogController(IBidLogService service)
        {
            _service = service;
            // _placeBidService = placeBidService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBidLog()
        {
            try
            {
                var bidLog = await _service.GetAllBidLog();
                if (bidLog == null)
                    return NotFound("No bid logs found.");

                var bidDtos = bidLog.Select(b => b.ToBidLogDtoFromBidLog());
                return Ok(bidDtos);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBidLogById([FromRoute] int id)
        {
            try
            {
                var bidLog = await _service.GetBidLogById(id);
                if (bidLog == null)
                    return NotFound($"BidLog with ID {id} does not exist.");

                return Ok(bidLog.ToBidLogDtoFromBidLog());
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // [HttpPost]
        // public async Task<IActionResult> PlaceBid(CreateBidLogDto placeBid)
        // {
        //     try
        //     {
        //         if (!ModelState.IsValid)
        //             return BadRequest(ModelState);

        //         var newBid = await _service.CreateBidLog(placeBid.ToBidLogFromCreateBidLogDto());
        //         var newBidDto = newBid.ToBidLogDtoFromBidLog();
        //         return CreatedAtAction(nameof(GetBidLogById), new { id = newBid.BidLogId }, newBidDto);
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        //     catch (Exception ex)
        //     {
        //         // Log the exception (optional)
        //         return StatusCode(500, "Internal server error: " + ex.Message);
        //     }
        // }
    }
}

