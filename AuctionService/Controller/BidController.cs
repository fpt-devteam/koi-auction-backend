using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Dto.AuctionLot;
using AuctionService.Dto.Wallet;
using AuctionService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/bid")]
    public class BidController : ControllerBase
    {
        private readonly BidManagementService _bidManagementService;
        private readonly WalletService _walletService;

        public BidController(BidManagementService bidManagementService, WalletService walletService)
        {
            _bidManagementService = bidManagementService;
            _walletService = walletService;
        }

        //payment
        [HttpPost("payment")]
        public async Task<IActionResult> Payment([FromBody] PaymentDto paymentDto)
        {
            if (paymentDto == null)
            {
                return BadRequest();
            }
            try
            {
                await _walletService.PaymentAsync(paymentDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        [HttpPost("update-balance")]
        public async Task<IActionResult> UpdateBalance([FromBody] WalletDto walletDto)
        {
            try
            {
                var balance = await _walletService.GetBalanceByIdAsync(walletDto.BidderId);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}