using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Productions.DTOs;
using CubArt.Application.Productions.Queries;
using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Productions.Handlers
{
    public class GetAllProductionsQueryHandler : IRequestHandler<GetAllProductionsQuery, Result<PagedListDto<ProductionDto>>>
    {
        private readonly IProductionRepository _productionRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Production>, IQueryable<Production>>> _sortMap = new()
        {
            ["productname"] = q => q.OrderBy(p => p.Product.Name),
            ["facilityname"] = q => q.OrderBy(p => p.Facility.Name),
            ["quantity"] = q => q.OrderBy(p => p.Quantity),
            ["datecreated"] = q => q.OrderBy(p => p.DateCreated)
        };


        public GetAllProductionsQueryHandler(
            IProductionRepository productionRepository,
            IMapper mapper)
        {
            _productionRepository = productionRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedListDto<ProductionDto>>> Handle(GetAllProductionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _productionRepository.GetQueryable()
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<ProductionDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedListDto<ProductionDto>>($"Ошибка получения производств: {ex.Message}", ex);
            }
        }

        private static IQueryable<Production> ApplyFilters(IQueryable<Production> query, GetAllProductionsQuery request)
        {
            if (request.ProductId.HasValue)
            {
                query = query.Where(p => p.ProductId == request.ProductId.Value);
            }

            if (request.FacilityId.HasValue)
            {
                query = query.Where(p => p.FacilityId == request.FacilityId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(p => p.DateCreated >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(p => p.DateCreated <= request.EndDate.Value);
            }

            return query;
        }

    }
}
