using CubArt.Api.Attributes;
using CubArt.Application.Common.Models;
using CubArt.Application.StockBalances.DTOs;
using CubArt.Application.StockBalances.Queries;
using CubArt.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CubArt.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class StockBalanceViewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockBalanceViewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [RequirePermission(PermissionEnum.StockRead)]
        [ProducesResponseType(typeof(PagedListDto<StockBalanceViewDto>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetStockBalanceViews([FromQuery] GetAllStockBalanceViewsQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
