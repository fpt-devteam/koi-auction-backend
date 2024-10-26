using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Dto.AuctionLot;
using BiddingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("api/bid")]
    public class BidController : ControllerBase
    {
        private readonly BidManagementService _bidManagementService;

        public BidController(BidManagementService bidManagementService)
        {
            _bidManagementService = bidManagementService;
        }

        [HttpPost("start-auction-lot")]
        public async Task<IActionResult> StartAuctionLot([FromBody] AuctionLotBidDto auctionLotBidDto)
        {
            try
            {
                await _bidManagementService.StartAuctionLot(auctionLotBidDto);
                return Ok(new { Message = "Start successfully", Data = auctionLotBidDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("end-auction-lot")]
        public async Task<IActionResult> EndAuctionLot()
        {
            try
            {
                await _bidManagementService.EndAuctionLot();
                return Ok(new { Message = "End successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }
}