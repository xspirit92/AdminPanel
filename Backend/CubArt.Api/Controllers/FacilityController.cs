using CubArt.Application.Common.Models;
using CubArt.Application.Facilities.DTOs;
using CubArt.Application.Facilities.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class FacilityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FacilityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(Result<List<FacilityDto>>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetFacilityList([FromQuery] GetFacilityListQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
