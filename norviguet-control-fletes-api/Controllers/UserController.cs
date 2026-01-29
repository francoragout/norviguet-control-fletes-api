using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.User;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await service.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id}/role")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> UpdateRole(int id, [FromBody] UserRoleUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.UpdateRoleAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await service.DeleteAsync([id], cancellationToken);
            return NoContent();
        }

        [HttpPost("bulk-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids, CancellationToken cancellationToken)
        {
            await service.DeleteAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
