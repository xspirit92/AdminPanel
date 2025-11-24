using CubArt.Application.Common.Models;
using CubArt.Application.Productions.DTOs;
using MediatR;

namespace CubArt.Application.Productions.Queries
{
    public record GetProductionByIdQuery(Guid Id) : IRequest<Result<ProductionDto>>;
}
