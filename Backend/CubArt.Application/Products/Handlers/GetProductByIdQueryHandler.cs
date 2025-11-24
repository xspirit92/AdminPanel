using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using CubArt.Application.Products.Queries;
using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Products.Handlers
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IMapper _mapper;

        public GetProductByIdQueryHandler(
            IRepository<Product, int> productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _productRepository.GetQueryable()
                    .Include(p => p.ProductSpecifications.Where(ps => ps.IsActive))
                    .ThenInclude(ps => ps.Items)
                    .ThenInclude(psi => psi.Product);

                var product = await query.FirstOrDefaultAsync(p => p.Id == request.Id);

                if (product is null)
                {
                    throw new NotFoundException(nameof(Product), request.Id);
                }

                var result = _mapper.Map<ProductDto>(product);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProductDto>($"Ошибка получения продукции: {ex.Message}", ex);
            }
        }
    }

}
