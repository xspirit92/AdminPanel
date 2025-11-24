using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.DTOs;
using MediatR;

namespace CubArt.Application.Supplies.Queries
{
    public record GetSupplyByIdQuery(Guid Id) : IRequest<Result<SupplyDto>>;
}
