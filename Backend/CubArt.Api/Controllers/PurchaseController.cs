using CubArt.Application.Common.Models;
using CubArt.Application.Facilities.DTOs;
using CubArt.Application.Facilities.Queries;
using CubArt.Application.Purchases.Commands;
using CubArt.Application.Purchases.DTOs;
using CubArt.Application.Purchases.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<PurchaseDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetPurchases([FromQuery] GetAllPurchasesQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(List<PurchaseDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetPurchaseList([FromQuery] GetPurchaseListQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PurchaseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetPurchaseById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPurchaseByIdQuery(id), cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PurchaseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CreateOrUpdatePurchase([FromBody] CreateOrUpdatePurchaseCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(PurchaseDto), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> DeletePurchaseById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletePurchaseByIdCommand() 
            {
                Id = id
            }, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
