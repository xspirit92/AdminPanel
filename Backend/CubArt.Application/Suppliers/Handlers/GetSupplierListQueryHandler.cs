using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Suppliers.DTOs;
using CubArt.Application.Suppliers.Queries;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Suppliers.Handlers
{
    public class GetSupplierListQueryHandler : IRequestHandler<GetSupplierListQuery, Result<List<SupplierDto>>>
    {
        private readonly IRepository<Supplier, int> _supplierRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Supplier>, IQueryable<Supplier>>> _sortMap = new()
        {
            ["name"] = q => q.OrderBy(p => p.Name)
        };


        public GetSupplierListQueryHandler(
            IRepository<Supplier, int> supplierRepository,
            IMapper mapper)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<SupplierDto>>> Handle(GetSupplierListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _supplierRepository.GetQueryable()
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<SupplierDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToListAsync(cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<SupplierDto>>($"Ошибка получения поставщиков: {ex.Message}", ex);
            }
        }

        private static IQueryable<Supplier> ApplyFilters(IQueryable<Supplier> query, GetSupplierListQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));
            }

            return query;
        }

    }
}
