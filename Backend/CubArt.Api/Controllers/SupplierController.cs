using CubArt.Application.Common.Models;
using CubArt.Application.Suppliers.DTOs;
using CubArt.Application.Suppliers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupplierController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(Result<List<SupplierDto>>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetSupplierList([FromQuery] GetSupplierListQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
