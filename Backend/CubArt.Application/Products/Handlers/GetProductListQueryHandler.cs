using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using CubArt.Application.Products.Queries;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Facilities.Handlers
{
    public class GetProductListQueryHandler : IRequestHandler<GetProductListQuery, Result<List<ProductDto>>>
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Product>, IQueryable<Product>>> _sortMap = new()
        {
            ["name"] = q => q.OrderBy(p => p.Name)
        };


        public GetProductListQueryHandler(
            IRepository<Product, int> productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _productRepository.GetQueryable()
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<ProductDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToListAsync(cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<ProductDto>>($"Ошибка получения продуктов: {ex.Message}", ex);
            }
        }

        private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, GetProductListQuery request)
        {
            query = query.Where(p => p.ProductType == request.ProductType);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));
            }

            if (request.UnitOfMeasure.HasValue)
            {
                query = query.Where(p => p.UnitOfMeasure == request.UnitOfMeasure.Value);
            }

            return query;
        }

    }
}
