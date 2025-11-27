using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.Commands;
using CubArt.Application.Purchases.DTOs;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Purchases.Handlers
{
    public class GetPurchasePagedListQueryHandler : IRequestHandler<GetPurchasePagedListQuery, Result<PagedListDto<PurchaseDto>>>
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


        public GetPurchasePagedListQueryHandler(
            IPurchaseRepository purchaseRepository,
            IMapper mapper)
        {
            _purchaseRepository = purchaseRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedListDto<PurchaseDto>>> Handle(GetPurchasePagedListQuery request, CancellationToken cancellationToken)
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
                var result = await projectedQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedListDto<PurchaseDto>>($"Ошибка получения закупок: {ex.Message}", ex);
            }
        }

        private static IQueryable<Purchase> ApplyFilters(IQueryable<Purchase> query, GetPurchasePagedListQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query
                    .Where(p =>
                        p.Supplier.Name.ToLower().Contains(request.SearchTerm.ToLower()) ||
                        p.Product.Name.ToLower().Contains(request.SearchTerm.ToLower()) ||
                        p.Facility.Name.ToLower().Contains(request.SearchTerm.ToLower())
                    );
            }

            if (request.SupplierId.HasValue)
            {
                query = query.Where(p => p.SupplierId == request.SupplierId.Value);
            }

            if (request.FacilityId.HasValue)
            {
                query = query.Where(p => p.FacilityId == request.FacilityId.Value);
            }

            if (request.ProductId.HasValue)
            {
                query = query.Where(p => p.ProductId == request.ProductId.Value);
            }

            if (request.PurchaseStatus.HasValue)
            {
                query = query.Where(p => p.PurchaseStatus == request.PurchaseStatus.Value);
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
