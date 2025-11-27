using CubArt.Application.Common.Models;
using CubArt.Application.Users.Commands;
using CubArt.Application.Users.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(Result<LoginDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
