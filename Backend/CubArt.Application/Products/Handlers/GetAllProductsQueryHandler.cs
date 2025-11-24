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

namespace CubArt.Application.Products.Handlers
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PagedListDto<ProductDto>>>
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Product>, IQueryable<Product>>> _sortMap = new()
        {
            ["name"] = q => q.OrderBy(p => p.Name),
            ["producttype"] = q => q.OrderBy(p => p.ProductType),
            ["unitofmeasure"] = q => q.OrderBy(p => p.UnitOfMeasure)
        };

        public GetAllProductsQueryHandler(
            IRepository<Product, int> productRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedListDto<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _productRepository.GetQueryable()
                    .Include(p => p.ProductSpecifications.Where(ps => ps.IsActive))
                    .ThenInclude(ps => ps.Items)
                    .ThenInclude(psi => psi.Product)
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<ProductDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedListDto<ProductDto>>($"Ошибка получения продукции: {ex.Message}", ex);
            }
        }

        private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, GetAllProductsQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));
            }

            if (request.ProductType.HasValue)
            {
                query = query.Where(p => p.ProductType == request.ProductType.Value);
            }

            if (request.UnitOfMeasure.HasValue)
            {
                query = query.Where(p => p.UnitOfMeasure == request.UnitOfMeasure.Value);
            }

            return query;
        }
    }

}
