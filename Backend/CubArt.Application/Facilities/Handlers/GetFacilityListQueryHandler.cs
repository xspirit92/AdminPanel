using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Facilities.DTOs;
using CubArt.Application.Facilities.Queries;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Facilities.Handlers
{
    public class GetFacilityListQueryHandler : IRequestHandler<GetFacilityListQuery, Result<List<FacilityDto>>>
    {
        private readonly IRepository<Facility, int> _facilityRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Facility>, IQueryable<Facility>>> _sortMap = new()
        {
            ["name"] = q => q.OrderBy(p => p.Name)
        };


        public GetFacilityListQueryHandler(
            IRepository<Facility, int> facilityRepository,
            IMapper mapper)
        {
            _facilityRepository = facilityRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<FacilityDto>>> Handle(GetFacilityListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _facilityRepository.GetQueryable()
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<FacilityDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToListAsync(cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<FacilityDto>>($"Ошибка получения производств: {ex.Message}", ex);
            }
        }

        private static IQueryable<Facility> ApplyFilters(IQueryable<Facility> query, GetFacilityListQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(request.Name.ToLower()));
            }

            return query;
        }

    }
}
