using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.BidLog;
using AuctionService.Helper;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.BidLog;
using AuctionService.Helper;
using AuctionService.IServices;
using AuctionService.Mapper;
using AuctionService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AuctionService.Controllers
{
    [Route("api/bid-log")]
    [ApiController]
    public class BidLogController : ControllerBase
    {
        private readonly IBidLogService _service;
namespace AuctionService.Controllers
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
            public async Task<IActionResult> GetAllBidLog([FromQuery] BidLogQueryObject queryObject)
            {
                var bidLog = await _service.GetAllBidLog(queryObject);
                if (bidLog == null)
                    return NotFound("No bid logs found.");

                var bidDtos = bidLog.Select(b => b.ToBidLogDtoFromBidLog());
                return Ok(bidDtos);
            }

            [HttpGet("{id:int}")]
            public async Task<IActionResult> GetBidLogById([FromRoute] int id)
            {
                var bidLog = await _service.GetBidLogById(id);
                if (bidLog == null)
                    return NotFound($"BidLog with ID {id} does not exist.");

                return Ok(bidLog.ToBidLogDtoFromBidLog());
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

