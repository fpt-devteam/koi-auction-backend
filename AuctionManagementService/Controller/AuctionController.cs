using AuctionManagementService.Dto.Auction;
using AuctionManagementService.Helper;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace AuctionManagementService.Controller
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository _repo;
        public AuctionController(IAuctionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuction([FromQuery] AuctionQueryObject query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auctions = await _repo.GetAllAsync(query);
            var acutionDtos = auctions.Select(d => d.ToAuctionDtoFromAuction());
            return Ok(acutionDtos);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auction = await _repo.GetByIdAsync(id);
            if(auction == null)
                return NotFound();
            return Ok(auction.ToAuctionDtoFromAuction());           
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto auctionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var action = auctionDto.ToAuctionFromCreateAuctionDto();
            await _repo.CreateAsync(action);
            return CreatedAtAction(nameof(GetAuctionById), new{id = action.AuctionId}, action.ToAuctionDtoFromAuction());

        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateAuction([FromRoute] int id, [FromBody] UpdateAuctionDto auctionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var action = await _repo.UpdateAsync(id, auctionDto);
            return Ok(action.ToAuctionDtoFromAuction());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult> DeleteAuction([FromRoute] int id)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var deletaAuction = await _repo.DeleteAsync(id);
            if(deletaAuction == null)
                return NotFound();
            return NoContent(); 
        }
    }
}