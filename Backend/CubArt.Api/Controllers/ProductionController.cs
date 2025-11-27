using CubArt.Application.Common.Models;
using CubArt.Application.Productions.Commands;
using CubArt.Application.Productions.DTOs;
using CubArt.Application.Productions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Result<PagedListDto<ProductionDto>>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetProductions([FromQuery] GetProductionPagedListQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<ProductionDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetProductionById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductionByIdQuery(id), cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Result<ProductionDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CreateOrUpdateProduction([FromBody] CreateOrUpdateProductionCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(Result<ProductionDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> DeleteProductionById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProductionByIdCommand()
            {
                Id = id
            }, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
