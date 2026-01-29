using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.Account;
using norviguet_control_fletes_api.Services.Interfaces;
using System.Security.Claims;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController(IAccountService service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> GetById(CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.GetByIdAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("name")]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> UpdateName([FromBody] AccountNameUpdateDto dto, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.UpdateNameAsync(userId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("password")]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountDto>> UpdatePassword([FromBody] AccountPasswordUpdateDto dto, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.UpdatePasswordAsync(userId, dto, cancellationToken);
            return Ok(result);
        }
    }
}
