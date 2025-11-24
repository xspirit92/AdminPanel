using CubArt.Domain.Common;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using CubArt.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CubArt.Infrastructure.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IRepository<StockMovement, Guid> _movementRepository;
        private readonly IRepository<StockBalance, Guid> _balanceRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IDapperService _dapperService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StockMovementService> _logger;

        public StockMovementService(
            IRepository<StockMovement, Guid> movementRepository,
            IRepository<StockBalance, Guid> balanceRepository,
            AppDbContext appDbContext,
            IDapperService dapperService,
            ILogger<StockMovementService> logger,
            IUnitOfWork unitOfWork)
        {
            _movementRepository = movementRepository;
            _balanceRepository = balanceRepository;
            _appDbContext = appDbContext;
            _dapperService = dapperService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<StockMovement>> GetStockMovementsByReference(string referenceId, StockMovemetReferenceTypeEnum referenceType)
        {
            var ers = await _movementRepository.GetQueryable()
                .Include(m => m.Facility)
                .Include(m => m.Product)
                .Where(m => m.ReferenceId == referenceId &&
                           m.ReferenceType == referenceType)
                .OrderBy(m => m.DateCreated)
                .ToListAsync();

            return ers;
        }

        public async Task RecalculateAllBalancesFromDate(DateTime date, int? facilityId = null, int? productId = null, CancellationToken cancellationToken = default)
        {
            while (date.Date < DateTime.UtcNow.Date || cancellationToken.IsCancellationRequested)
            {
                await RecalculateAllBalancesForDate(date, facilityId, productId, cancellationToken);
                _logger.LogInformation("Балансы пересчитаны за {Date}", date.ToString("yyyy-MM-dd"));

                date = date.AddDays(1).Date;
            }
        }

        public async Task<DateTime> GetLastBalanceDate()
        {
            var lastBalanceDate = await _balanceRepository.GetQueryable()
                .OrderByDescending(b => b.DateCreated)
                .Select(b => b.DateCreated.Date)
                .FirstOrDefaultAsync();

            var today = DateTime.UtcNow.Date;
            return lastBalanceDate == default ? today.AddDays(-7) : lastBalanceDate.AddDays(1);
        }

        public async Task<StockBalanceView> GetStockBalanceByDate(int? facilityId, int? productId, DateTime balanceDate)
        {
            var sql = $@"
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
                      (COALESCE(lb.finish_balance, 0) != 0 OR COALESCE(dm.income, 0) != 0 OR COALESCE(dm.outcome, 0) != 0)";

            // Создаем параметры
            var parameters = new
            {
                balanceDate = balanceDate.Date,
                facilityId = facilityId,
                productId = productId
            };

            // Получаем данные 
            return await _dapperService.QueryFirstOrDefaultAsync<StockBalanceView>(sql, parameters);
        }

        public async Task UpdateStockMovementsAndRecalculateBalances(UpdateStockMovementsAndRecalculateBalancesModel model)
        {
            var movements = await GetStockMovementsByReference(model.ReferenceId.ToString(), model.ReferenceType);
            foreach (var movement in movements)
            {                
                movement.UpdateEntity(
                    model.FacilityId,
                    model.ProductId,
                    model.OperationType,
                    model.ReferenceType,
                    model.ReferenceId.ToString(),
                    model.Quantity,
                    model.Date
                );
                _movementRepository.Update(movement);
            }
            await _unitOfWork.CommitAsync();
            await RecalculateAllBalancesFromDate(model.Date.Date, model.FacilityId, model.ProductId);
        }

        public async Task DeleteStockMovements(Guid referenceId, StockMovemetReferenceTypeEnum referenceType)
        {
            var movements = await GetStockMovementsByReference(referenceId.ToString(), referenceType);
            foreach (var movement in movements)
            {
                var entity = await _movementRepository.GetByIdAsync(movement.Id);
                _movementRepository.Delete(entity);
            }
            await _unitOfWork.CommitAsync();
        }

        #region Private methods

        private async Task RecalculateAllBalancesForDate(DateTime date, int? facilityId = null, int? productId = null, CancellationToken cancellationToken = default)
        {
            // Получаем все уникальные комбинации facilityId + productId за текущую дату
            var movementCombinations = await _movementRepository.GetQueryable()
                .Where(m => m.DateCreated.Date == date.Date &&
                    (facilityId == null || m.FacilityId == facilityId) &&
                    (productId == null || m.ProductId == productId))
                .Select(m => new { m.FacilityId, m.ProductId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var balanceCombinations = await _balanceRepository.GetQueryable()
                .Where(m => m.DateCreated.Date == date.Date &&
                    (facilityId == null || m.FacilityId == facilityId) &&
                    (productId == null || m.ProductId == productId))
                .Select(m => new { m.Id, m.FacilityId, m.ProductId })
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var combo in balanceCombinations)
            {
                if (!movementCombinations.Any(x => x.FacilityId == combo.FacilityId && x.ProductId == combo.ProductId))
                {
                    var entity = await _balanceRepository.GetByIdAsync(combo.Id);
                    _balanceRepository.Delete(entity);
                }
            }
            await _unitOfWork.CommitAsync();

            foreach (var combo in movementCombinations)
            {
                await RecalculateBalance(combo.FacilityId, combo.ProductId, date, cancellationToken);
            }
        }

        private async Task RecalculateBalance(int facilityId, int productId, DateTime date,
            CancellationToken cancellationToken = default)
        {
            // Получаем все движения за указанную дату
            var movements = await _movementRepository.GetQueryable()
                .Where(m => m.FacilityId == facilityId &&
                           m.ProductId == productId &&
                           m.DateCreated.Date == date.Date)
                .OrderBy(m => m.DateCreated)
                .ToListAsync(cancellationToken);

            if (!movements.Any()) return;

            // Находим последний баланс перед началом периода
            var lastBalance = await _balanceRepository.GetQueryable()
                .Where(b => b.FacilityId == facilityId &&
                           b.ProductId == productId &&
                           b.DateCreated < date.Date)
                .OrderByDescending(b => b.DateCreated)
                .FirstOrDefaultAsync(cancellationToken);

            decimal startBalance = lastBalance?.FinishBalance ?? 0;
            decimal income = 0;
            decimal outcome = 0;

            foreach (var movement in movements)
            {
                if (movement.OperationType == OperationTypeEnum.Income)
                    income += movement.Quantity;
                else
                    outcome += movement.Quantity;
            }

            decimal finishBalance = startBalance + income - outcome;

            // Создаем или обновляем баланс
            var existingBalance = await _balanceRepository
                .GetQueryable()
                .FirstOrDefaultAsync(b => b.FacilityId == facilityId &&
                                        b.ProductId == productId &&
                                        b.DateCreated.Date == date.Date,
                                    cancellationToken);

            if (existingBalance != null)
            {
                existingBalance.UpdateBalances(startBalance, income, outcome, finishBalance);
                _balanceRepository.Update(existingBalance);
            }
            else
            {
                var balance = new StockBalance(
                    facilityId, productId, startBalance, income, outcome, finishBalance, date.Date);
                await _balanceRepository.AddAsync(balance);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        #endregion
    }
}
