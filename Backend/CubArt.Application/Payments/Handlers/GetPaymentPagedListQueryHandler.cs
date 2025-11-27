using AutoMapper;
using AutoMapper.QueryableExtensions;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Models;
using CubArt.Application.Payments.DTOs;
using CubArt.Application.Payments.Queries;
using CubArt.Domain.Entities;
using CubArt.Infrastructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Payments.Handlers
{
    public class GetPaymentPagedListQueryHandler : IRequestHandler<GetPaymentPagedListQuery, Result<PagedListDto<PaymentDto>>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Func<IQueryable<Payment>, IQueryable<Payment>>> _sortMap = new()
        {
            ["purchasename"] = q => q.OrderBy(p => $"{p.Purchase.Product.Name} от {p.Purchase.DateCreated.ToString("dd.MM.yyyy")} - {p.Purchase.Supplier.Name} ({p.Purchase.Amount} руб.)"),
            ["amount"] = q => q.OrderBy(p => p.Amount),
            ["paymentmethod"] = q => q.OrderBy(p => p.PaymentMethod),
            ["paymentstatus"] = q => q.OrderBy(p => p.PaymentStatus),
            ["datecreated"] = q => q.OrderBy(p => p.DateCreated)
        };


        public GetPaymentPagedListQueryHandler(
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedListDto<PaymentDto>>> Handle(GetPaymentPagedListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                var query = _paymentRepository.GetQueryable()
                    .Include(x => x.Purchase)
                        .ThenInclude(x => x.Product)
                    .Include(x => x.Purchase)
                        .ThenInclude(x => x.Supplier)
                    .AsQueryable();

                // Фильтрация
                query = ApplyFilters(query, request);

                // Сортировка
                query = query.ApplySorting(request.SortBy, request.SortDescending, _sortMap);

                // Проекция и пагинация
                var projectedQuery = query.ProjectTo<PaymentDto>(_mapper.ConfigurationProvider);
                var result = await projectedQuery.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedListDto<PaymentDto>>($"Ошибка получения оплат: {ex.Message}", ex);
            }
        }

        private static IQueryable<Payment> ApplyFilters(IQueryable<Payment> query, GetPaymentPagedListQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query
                    .Where(p =>
                        p.Purchase.Product.Name.ToLower().Contains(searchTerm) ||
                        p.Purchase.Supplier.Name.ToLower().Contains(searchTerm) ||
                        p.Purchase.Amount.ToString().Contains(searchTerm)
                    );

            }

            if (request.PurchaseId.HasValue)
            {
                query = query.Where(p => p.PurchaseId == request.PurchaseId.Value);
            }

            if (request.PaymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == request.PaymentMethod.Value);
            }

            if (request.PaymentStatus.HasValue)
            {
                query = query.Where(p => p.PaymentStatus == request.PaymentStatus.Value);
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
