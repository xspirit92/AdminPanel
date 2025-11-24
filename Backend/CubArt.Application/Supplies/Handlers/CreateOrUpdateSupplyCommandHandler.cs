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
using CubArt.Infrastructure.Models;
using MediatR;

namespace CubArt.Application.Supplies.Handlers
{
    public class CreateOrUpdateSupplyCommandHandler : IRequestHandler<CreateOrUpdateSupplyCommand, Result<SupplyDto>>
    {
        private readonly ISupplyRepository _supplyRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IRepository<StockMovement, Guid> _stockMovementRepository;
        private readonly IStockMovementService _stockMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateOrUpdateSupplyCommandHandler(
            ISupplyRepository supplyRepository,
            IPurchaseRepository purchaseRepository,
            IRepository<StockMovement, Guid> stockMovementRepository,
            IStockMovementService stockMovementService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _supplyRepository = supplyRepository;
            _purchaseRepository = purchaseRepository;
            _stockMovementRepository = stockMovementRepository;
            _stockMovementService = stockMovementService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<SupplyDto>> Handle(CreateOrUpdateSupplyCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Проверяем существование закупки
                var purchase = await _purchaseRepository.GetByIdAsync(request.PurchaseId);
                if (purchase == null)
                {
                    throw new NotFoundException(nameof(Purchase), request.PurchaseId);
                }
                if (purchase.PurchaseStatus == PurchaseStatusEnum.Completed)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<SupplyDto>($"Ошибка при сохранении поставки: Закупка данной поставки в статусе 'Завершена'");
                }

                var supplyId = request.Id;
                // Проверяем, не превышает ли количество оставшееся
                var totalQuantity = await _supplyRepository.GetTotalIncomeQuantityAsync(request.PurchaseId, supplyId);
                var remainingQuantity = purchase.Quantity - totalQuantity;

                if (request.Quantity > remainingQuantity)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<SupplyDto>($"Количество в поставке {request.Quantity} превосходит оставшееся {remainingQuantity}");
                }

                Supply? supply;
                if (supplyId is null)
                {
                    // Создаем поставку
                    supply = new Supply(request.PurchaseId, request.Quantity);

                    await _supplyRepository.AddAsync(supply);
                    await _unitOfWork.CommitAsync(cancellationToken);

                    // Создаем движение запасов
                    var movement = new StockMovement(
                        facilityId: purchase.FacilityId,
                        productId: purchase.ProductId,
                        operationType: OperationTypeEnum.Income,
                        referenceType: StockMovemetReferenceTypeEnum.Supply,
                        referenceId: supply.Id.ToString(),
                        quantity: request.Quantity,
                        dateCreated: supply.DateCreated);

                    await _stockMovementRepository.AddAsync(movement);
                }
                else
                {
                    // Обновление поставки
                    supply = await _supplyRepository.GetByIdAsync(supplyId.Value);
                    if (supply == null)
                    {
                        throw new NotFoundException(nameof(Supply), supplyId.Value);
                    }

                    supply.UpdateEntity(
                        request.PurchaseId,
                        request.Quantity
                    );

                    _supplyRepository.Update(supply);

                    await _stockMovementService.UpdateStockMovementsAndRecalculateBalances(
                        new UpdateStockMovementsAndRecalculateBalancesModel(
                            supply.Id,
                            OperationTypeEnum.Income,
                            StockMovemetReferenceTypeEnum.Supply,
                            supply.DateCreated,
                            purchase.FacilityId,
                            purchase.ProductId,
                            request.Quantity
                        )                        
                    );
                }

                // Если поставка полная, то переводим статус закупки
                if (remainingQuantity - request.Quantity == 0)
                {
                    purchase.PurchaseStatus = PurchaseStatusEnum.Completed;
                    _purchaseRepository.Update(purchase);
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                // Получаем полные данные для ответа
                var fullSupply = await _supplyRepository.GetByIdAsync(supply.Id);

                return Result.Success(_mapper.Map<SupplyDto>(fullSupply));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return Result.Failure<SupplyDto>($"Ошибка при создании поставки: {ex.Message}", ex);
            }
        }
    }
}
