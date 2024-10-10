using AuctionService.Dto.Auction;
using AuctionService.Helper;
using AuctionService.IRepository;
using AuctionService.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace AuctionService.Controller
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BreederDetailController _breederDetailController;

        public AuctionController(IUnitOfWork unitOfWork, BreederDetailController breederDetailController)
        {
            _unitOfWork = unitOfWork;
            _breederDetailController = breederDetailController;
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
            if (auction == null)
                return NotFound();
            return Ok(auction.ToAuctionDtoFromAuction());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto auctionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var auction = auctionDto.ToAuctionFromCreateAuctionDto();
            auction.AuctionName = AuctionHelper.GenerateAuctionName(auction);
            await _unitOfWork.Auctions.CreateAsync(auction);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.AuctionId }, auction.ToAuctionDtoFromAuction());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateAuction([FromRoute] int id, [FromBody] UpdateAuctionDto auctionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var action = await _unitOfWork.Auctions.UpdateAsync(id, auctionDto);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return Ok(action.ToAuctionDtoFromAuction());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult> DeleteAuction([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var deletedAuction = await _unitOfWork.Auctions.GetByIdAsync(id);
            if (deletedAuction == null)
                return NotFound();
            var auctionLots = deletedAuction.AuctionLots;
            if (auctionLots != null)
            {
                foreach (var acutionLot in auctionLots)
                {
                    await _unitOfWork.AuctionLots.DeleteAsync(acutionLot.AuctionLotId);
                }
            }
            await _unitOfWork.Auctions.DeleteAsync(id);
            if (!await _unitOfWork.SaveChangesAsync())
            {
                return BadRequest("An error occurred while saving the data");
            }
            return NoContent();
        }
    }
}