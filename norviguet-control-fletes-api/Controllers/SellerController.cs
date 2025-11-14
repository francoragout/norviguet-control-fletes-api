using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Common;
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

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<SellerDto>> GetSeller(int id)
        {
            var seller = await _context.Sellers.FindAsync(id);
            if (seller == null)
                return NotFound();
            var result = _mapper.Map<SellerDto>(seller);
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
            var seller = await _context.Sellers
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (seller == null)
                return NotFound();
            if (seller.Orders?.Any() == true)
            {
                return Conflict(new {
                    code = "ASSOCIATED_RECORDS",
                    message = "Seller cannot be deleted because it has associated orders."
                });
            }
            _context.Sellers.Remove(seller);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteSellers([FromBody] DeleteEntitiesDto dto)
        {
            var sellers = await _context.Sellers
                .Where(s => dto.Ids.Contains(s.Id))
                .Include(s => s.Orders)
                .ToListAsync();

            if (sellers.Count == 0)
                return NotFound();

            var cannotDelete = sellers
                .Where(s => s.Orders?.Any() == true)
                .ToList();

            if (cannotDelete.Any())
            {
                return Conflict(new {
                    code = "ASSOCIATED_RECORDS",
                    message = "Some sellers could not be deleted because they have associated orders."
                });
            }

            _context.Sellers.RemoveRange(sellers);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
