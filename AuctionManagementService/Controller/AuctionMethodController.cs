

using AuctionManagementService.Dto;
using AuctionManagementService.IRepository;
using AuctionManagementService.Mapper;
using Microsoft.AspNetCore.Mvc;

namespace AuctionManagementService.Controller
{
    [Route("api/auction-methods")]
    [ApiController]
    public class AuctionMethodController : ControllerBase
    {
        private readonly IAuctionMethodRepository _repo;
        public AuctionMethodController(IAuctionMethodRepository repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var methods = await _repo.GetAllAsync();
            var methodDtos = methods.Select(m => m.ToAuctionMethodDtoFromAuctionMethod());
            return Ok(methodDtos);
        }
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAuctionMethodByID([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var method = await _repo.GetByIdAsync(id);
            if (method == null)
            {
                return NotFound();
            }
            return Ok(method.ToAuctionMethodDtoFromAuctionMethod());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMethodAuction([FromBody] CreateAuctionMethodDto methodDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var method = methodDto.ToActionMethodFromCreateAuctionMethodDto();
            var newMethod = await _repo.CreateAsync(method);
            return CreatedAtAction(nameof(GetAuctionMethodByID), new { id = newMethod.AuctionMethodId }, newMethod.ToAuctionMethodDtoFromAuctionMethod());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateMethodAuction([FromRoute] int id, [FromBody] UpdateAuctionMethodDto methodDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updateMethod = await _repo.UpdateAsync(id, methodDto);
            return Ok(updateMethod.ToAuctionMethodDtoFromAuctionMethod());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteMethodAuction([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var deleteMethod = await _repo.DeleteAsync(id);
            if (deleteMethod == null)
                return NotFound();
            return NoContent();
        }


    }


}