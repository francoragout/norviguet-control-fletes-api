using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Seller;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SellerController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public SellerController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<SellerDto>>> GetSellers()
        {
            var sellers = await _context.Sellers.ToListAsync();
            var result = _mapper.Map<List<SellerDto>>(sellers);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> CreateSeller([FromBody] CreateSellerDto dto)
        {
            var seller = _mapper.Map<Entities.Seller>(dto);
            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateSeller(int id, [FromBody] UpdateSellerDto dto)
        {
            var existingSeller = await _context.Sellers.FindAsync(id);
            if (existingSeller == null)
                return NotFound();
            _mapper.Map(dto, existingSeller);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteSeller(int id)
        {
            var existingSeller = await _context.Sellers.FindAsync(id);
            if (existingSeller == null)
                return NotFound();
            _context.Sellers.Remove(existingSeller);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteSellers([FromBody] DeleteSellersDto dto)
        {
            var sellersToDelete = await _context.Sellers
                .Where(s => dto.Ids.Contains(s.Id))
                .ToListAsync();
            if (sellersToDelete.Count == 0)
                return NotFound();
            _context.Sellers.RemoveRange(sellersToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
