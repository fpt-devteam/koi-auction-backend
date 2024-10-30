using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controller
{
    [ApiController]
    [Route("api/sold-lots")]
    public class SoldLotController : ControllerBase
    {
        //get sold lot by id
        private readonly ISoldLotService _service;
        public SoldLotController(ISoldLotService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSoldLotById([FromRoute] int id)
        {
            try
            {
                var soldLot = await _service.GetSoldLotById(id);
                if (soldLot == null)
                    return NotFound($"SoldLot with ID {id} does not exist.");

                return Ok(soldLot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}