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
        private readonly IUnitOfWork _unitOfWork;
        public AuctionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuction([FromQuery] AuctionQueryObject query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auctions = await _unitOfWork.Auctions.GetAllAsync(query);
            var acutionDtos = auctions.Select(d => d.ToAuctionDtoFromAuction());
            return Ok(acutionDtos);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auction = await _unitOfWork.Auctions.GetByIdAsync(id);
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
            await _unitOfWork.Auctions.CreateAsync(action);
            _unitOfWork.SaveChanges();
            return CreatedAtAction(nameof(GetAuctionById), new{id = action.AuctionId}, action.ToAuctionDtoFromAuction());

        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateAuction([FromRoute] int id, [FromBody] UpdateAuctionDto auctionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var action = await _unitOfWork.Auctions.UpdateAsync(id, auctionDto);
            _unitOfWork.SaveChanges();
            return Ok(action.ToAuctionDtoFromAuction());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult> DeleteAuction([FromRoute] int id)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var deletaAuction = await _unitOfWork.Auctions.DeleteAsync(id);
            if(deletaAuction == null)
                return NotFound();
            _unitOfWork.SaveChanges();
            return NoContent(); 
        }
    }
}