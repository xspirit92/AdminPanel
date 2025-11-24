using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.DTOs;
using MediatR;

namespace CubArt.Application.Purchases.Commands
{
    public record GetPurchaseByIdQuery(Guid Id) : IRequest<Result<PurchaseDto>>;
}
