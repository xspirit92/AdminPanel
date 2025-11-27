using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.DTOs;
using CubArt.Application.Purchases.Queries;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Facilities.Handlers
{
    public class GetPurchaseListQueryHandler : IRequestHandler<GetPurchaseListQuery, Result<List<PurchaseDto>>>
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Purchase>, IQueryable<Purchase>>> _sortMap = new()
        {
            ["amount"] = q => q.OrderBy(p => p.Amount),
            ["quantity"] = q => q.OrderBy(p => p.Quantity),
            ["datecreated"] = q => q.OrderBy(p => p.DateCreated),
            ["purchasestatus"] = q => q.OrderBy(p => p.PurchaseStatus),
            ["suppliername"] = q => q.OrderBy(p => p.Supplier.Name),
            ["facilityname"] = q => q.OrderBy(p => p.Facility.Name),
            ["productname"] = q => q.OrderBy(p => p.Product.Name)
        };


        public GetPurchaseListQueryHandler(
            IPurchaseRepository purchaseRepository,
            IMapper mapper)
        {
            _purchaseRepository = purchaseRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<PurchaseDto>>> Handle(GetPurchaseListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _purchaseRepository.GetQueryable()
                    .Include(x => x.Product)
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<PurchaseDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToListAsync(cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<PurchaseDto>>($"Ошибка получения производств: {ex.Message}", ex);
            }
        }

        private static IQueryable<Purchase> ApplyFilters(IQueryable<Purchase> query, GetPurchaseListQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(p => p.Product.Name.ToLower().Contains(request.Name.ToLower()));
            }

            return query;
        }

    }
}
