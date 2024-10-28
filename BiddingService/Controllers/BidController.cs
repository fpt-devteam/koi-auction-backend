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



    }
}