using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using MediatR;

namespace CubArt.Application.Products.Queries
{
    public record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;
}
