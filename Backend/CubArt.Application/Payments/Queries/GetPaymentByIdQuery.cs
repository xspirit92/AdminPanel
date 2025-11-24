using CubArt.Application.Common.Models;
using CubArt.Application.Payments.DTOs;
using MediatR;

namespace CubArt.Application.Payments.Queries
{
    public record GetPaymentByIdQuery(Guid Id) : IRequest<Result<PaymentDto>>;
}
