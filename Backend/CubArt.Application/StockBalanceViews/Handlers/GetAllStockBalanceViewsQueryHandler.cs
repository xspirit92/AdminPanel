using CubArt.Application.Common.Models;
using CubArt.Application.StockBalances.DTOs;
using CubArt.Application.StockBalances.Queries;
using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.StockBalances.Handlers
{
    public class GetAllStockBalanceViewsQueryHandler : IRequestHandler<GetAllStockBalanceViewsQuery, Result<PagedListDto<StockBalanceViewDto>>>
    {
        private readonly IDapperService _dapperService;
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Facility, int> _facilityRepository;

        public GetAllStockBalanceViewsQueryHandler(
            IDapperService dapperService,
            IRepository<Product, int> productRepository,
            IRepository<Facility, int> facilityRepository)
        {
            _dapperService = dapperService;
            _productRepository = productRepository;
            _facilityRepository = facilityRepository;
        }

        public async Task<Result<PagedListDto<StockBalanceViewDto>>> Handle(GetAllStockBalanceViewsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.Normalize();

                // Проверяем существование производтсва
                if (request.FacilityId.HasValue)
                {
                    var facility = await _facilityRepository.GetByIdAsync(request.FacilityId.Value);
                    if (facility == null)
                    {
                        throw new NotFoundException(nameof(Facility), request.FacilityId);
                    }
                }

                // Проверяем существование продукции
                if (request.ProductId.HasValue)
                {
                    var product = await _productRepository.GetByIdAsync(request.ProductId.Value);
                    if (product == null)
                    {
                        throw new NotFoundException(nameof(Product), request.ProductId);
                    }
                }

                // Получаем общее количество записей
                var countSql = @"
                    SELECT COUNT(*)
                    FROM facility f
                    CROSS JOIN product p
                    LEFT JOIN (
                        SELECT DISTINCT ON (facility_id, product_id) 
                            facility_id, 
                            product_id, 
                            finish_balance
                        FROM stock_balance 
                        WHERE date_created < @balanceDate
                        ORDER BY facility_id, product_id, date_created DESC
                    ) lb ON lb.facility_id = f.id AND lb.product_id = p.id
                    LEFT JOIN (
                        SELECT 
                            facility_id,
                            product_id,
                            COALESCE(SUM(CASE WHEN operation_type = 1 THEN quantity ELSE 0 END), 0) as income,
                            COALESCE(SUM(CASE WHEN operation_type = 2 THEN quantity ELSE 0 END), 0) as outcome
                        FROM stock_movement 
                        WHERE DATE(date_created) = DATE(@balanceDate)
                        GROUP BY facility_id, product_id
                    ) dm ON dm.facility_id = f.id AND dm.product_id = p.id
                    WHERE 
                      (@facilityId IS NULL OR f.id = @facilityId) AND
                      (@productId IS NULL OR p.id = @productId) AND
                      (@productType IS NULL OR p.product_type = @productType) AND
                      (COALESCE(lb.finish_balance, 0) != 0 OR COALESCE(dm.income, 0) != 0 OR COALESCE(dm.outcome, 0) != 0)";

                // Основной запрос с сортировкой и пагинацией
                var orderByClause = GetOrderByClause(request.SortBy, request.SortDescending);
                var mainSql = $@"
                    WITH last_balances AS (
                        SELECT DISTINCT ON (facility_id, product_id) 
                            facility_id, 
                            product_id, 
                            finish_balance
                        FROM stock_balance 
                        WHERE date_created < @balanceDate
                        ORDER BY facility_id, product_id, date_created DESC
                    ),
                    day_movements AS (
                        SELECT 
                            facility_id,
                            product_id,
                            COALESCE(SUM(CASE WHEN operation_type = 1 THEN quantity ELSE 0 END), 0) as income,
                            COALESCE(SUM(CASE WHEN operation_type = 2 THEN quantity ELSE 0 END), 0) as outcome
                        FROM stock_movement 
                        WHERE DATE(date_created) = DATE(@balanceDate)
                        GROUP BY facility_id, product_id
                    )
                    SELECT 
                        f.name as FacilityName,
                        p.name as ProductName,
                        p.product_type as ProductType,
                        p.unit_of_measure as UnitOfMeasure,
                        COALESCE(lb.finish_balance, 0) as StartBalance,
                        COALESCE(dm.income, 0) as IncomeBalance,
                        COALESCE(dm.outcome, 0) as OutcomeBalance,
                        COALESCE(lb.finish_balance, 0) + COALESCE(dm.income, 0) - COALESCE(dm.outcome, 0) as FinishBalance,
                        DATE(@balanceDate) as BalanceDate
                    FROM facility f
                    CROSS JOIN product p
                    LEFT JOIN last_balances lb ON lb.facility_id = f.id AND lb.product_id = p.id
                    LEFT JOIN day_movements dm ON dm.facility_id = f.id AND dm.product_id = p.id
                    WHERE 
                      (@facilityId IS NULL OR f.id = @facilityId) AND
                      (@productId IS NULL OR p.id = @productId) AND
                      (@productType IS NULL OR p.product_type = @productType) AND
                      (COALESCE(lb.finish_balance, 0) != 0 OR COALESCE(dm.income, 0) != 0 OR COALESCE(dm.outcome, 0) != 0)
                    {orderByClause}
                    LIMIT @pageSize OFFSET @offset";

                // Создаем параметры
                var parameters = new
                {
                    balanceDate = request.BalanceDate,
                    facilityId = request.FacilityId,
                    productId = request.ProductId,
                    productType = request.ProductType.HasValue ? (int)request.ProductType.Value : (int?)null,
                    pageSize = request.PageSize,
                    offset = (request.PageNumber - 1) * request.PageSize
                };

                // Получаем общее количество
                var totalCount = await _dapperService.ExecuteScalarAsync<int>(countSql, parameters);

                // Получаем данные с пагинацией
                var items = await _dapperService.QueryAsync<StockBalanceViewDto>(mainSql, parameters);

                var result = PagedListDto<StockBalanceViewDto>.Create(items.ToList(), totalCount, request.PageNumber, request.PageSize);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedListDto<StockBalanceViewDto>>($"Ошибка получения остатков: {ex.Message}", ex);
            }
        }

        private string GetOrderByClause(string sortBy, bool sortDescending)
        {
            var direction = sortDescending ? "DESC" : "ASC";

            var orderBy = sortBy?.ToLower() switch
            {
                "facilityname" => "f.name",
                "productname" => "p.name",
                "producttype" => "p.product_type",
                "unitofmeasure" => "p.unit_of_measure",
                "startbalance" => "StartBalance",
                "incomebalance" => "IncomeBalance",
                "outcomebalance" => "OutcomeBalance",
                "finishbalance" => "FinishBalance",
                "balancedate" => "BalanceDate",
                _ => "p.name" // сортировка по умолчанию
            };

            return $"ORDER BY {orderBy} {direction}";
        }
    }
}