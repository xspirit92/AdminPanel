using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.Commands;
using CubArt.Application.Supplies.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Supplies.Handlers
{
    public class DeleteSupplyByIdCommandHandler : IRequestHandler<DeleteSupplyByIdCommand, Result>
    {
        private readonly ISupplyRepository _supplyRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IStockMovementService _stockMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteSupplyByIdCommandHandler(
            ISupplyRepository supplyRepository,
            IPurchaseRepository purchaseRepository,
            IStockMovementService stockMovementService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _supplyRepository = supplyRepository;
            _purchaseRepository = purchaseRepository;
            _stockMovementService = stockMovementService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> Handle(DeleteSupplyByIdCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Проверяем существование поставки
                var supply = await _supplyRepository.GetByIdAsync(request.Id);
                if (supply == null)
                {
                    throw new NotFoundException(nameof(Supply), request.Id);
                }

                // Проверяем закупку
                var purchase = await _purchaseRepository.GetByIdAsync(supply.PurchaseId);
                if (purchase == null)
                {
                    throw new NotFoundException(nameof(Purchase), supply.PurchaseId);
                }
                if (purchase.PurchaseStatus == PurchaseStatusEnum.Completed)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<SupplyDto>($"Ошибка при удалении поставки: Закупка данной поставки в статусе 'Завершена'");
                }

                _supplyRepository.Delete(supply);

                // Удаляем движения запасов
                await _stockMovementService.DeleteStockMovements(supply.Id, StockMovemetReferenceTypeEnum.Supply);

                // Пересчет балансов
                await _stockMovementService.RecalculateAllBalancesFromDate(supply.DateCreated.Date, purchase.FacilityId, purchase.ProductId);

                await _unitOfWork.CommitAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return Result.Failure<SupplyDto>($"Ошибка при удалении поставки: {ex.Message}", ex);
            }
        }
    }
}
