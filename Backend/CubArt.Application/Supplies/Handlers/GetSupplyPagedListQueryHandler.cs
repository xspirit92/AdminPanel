using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.DTOs;
using CubArt.Application.Supplies.Queries;
using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Supplies.Handlers
{
    public class GetSupplyPagedListQueryHandler : IRequestHandler<GetSupplyPagedListQuery, Result<PagedListDto<SupplyDto>>>
    {
        private readonly ISupplyRepository _supplyRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Supply>, IQueryable<Supply>>> _sortMap = new()
        {
            ["quantity"] = q => q.OrderBy(p => p.Quantity),
            ["datecreated"] = q => q.OrderBy(p => p.DateCreated)
        };


        public GetSupplyPagedListQueryHandler(
            ISupplyRepository supplyRepository,
            IMapper mapper)
        {
            _supplyRepository = supplyRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedListDto<SupplyDto>>> Handle(GetSupplyPagedListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _supplyRepository.GetQueryable()
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<SupplyDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedListDto<SupplyDto>>($"Ошибка получения поставок: {ex.Message}", ex);
            }
        }

        private static IQueryable<Supply> ApplyFilters(IQueryable<Supply> query, GetSupplyPagedListQuery request)
        {
            if (request.PurchaseId.HasValue)
            {
                query = query.Where(p => p.PurchaseId == request.PurchaseId.Value);
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
