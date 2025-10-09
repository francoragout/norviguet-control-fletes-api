using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Payment;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController(NorviguetDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly NorviguetDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<PaymentDto>>> GetPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Orders)
                .ToListAsync();
            var result = _mapper.Map<List<PaymentDto>>(payments);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();
            var result = _mapper.Map<PaymentDto>(payment);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var payment = _mapper.Map<Entities.Payment>(dto);
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<PaymentDto>(payment);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto dto)
        {
            var existingPayment = await _context.Payments.FindAsync(id);
            if (existingPayment == null)
                return NotFound();
            _mapper.Map(dto, existingPayment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var existingPayment = await _context.Payments.FindAsync(id);
            if (existingPayment == null)
                return NotFound();
            _context.Payments.Remove(existingPayment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeletePayments([FromBody] DeletePaymentsDto ids)
        {
            var payments = await _context.Payments.Where(p => ids.PaymentIds.Contains(p.Id)).ToListAsync();
            if (payments.Count == 0)
                return NotFound();
            _context.Payments.RemoveRange(payments);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
