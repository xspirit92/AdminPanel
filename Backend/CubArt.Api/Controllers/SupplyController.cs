using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.Commands;
using CubArt.Application.Supplies.DTOs;
using CubArt.Application.Supplies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SupplyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupplyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<SupplyDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetSupplies([FromQuery] GetAllSuppliesQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SupplyDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetSupplyById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSupplyByIdQuery(id), cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SupplyDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CreateOrUpdateSupply([FromBody] CreateOrUpdateSupplyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(SupplyDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> DeleteSupplyById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteSupplyByIdCommand()
            { 
                Id = id
            }, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
