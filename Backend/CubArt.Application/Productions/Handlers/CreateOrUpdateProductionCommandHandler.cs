using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Productions.Commands;
using CubArt.Application.Productions.DTOs;
using CubArt.Application.Productions.Queries;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using CubArt.Infrastructure.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Productions.Handlers
{
    public class CreateOrUpdateProductionCommandHandler : IRequestHandler<CreateOrUpdateProductionCommand, Result<ProductionDto>>
    {
        private readonly IProductionRepository _productionRepository;
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Facility, int> _facilityRepository;
        private readonly IRepository<ProductSpecification, int> _specificationRepository;
        private readonly IRepository<StockMovement, Guid> _stockMovementRepository;
        private readonly IStockMovementService _stockMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateOrUpdateProductionCommandHandler(
            IProductionRepository productionRepository,
            IRepository<Product, int> productRepository,
            IRepository<Facility, int> facilityRepository,
            IRepository<ProductSpecification, int> specificationRepository,
            IRepository<StockMovement, Guid> stockMovementRepository,
            IStockMovementService stockMovementService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IMediator mediator)

        {
            _productionRepository = productionRepository;
            _productRepository = productRepository;
            _facilityRepository = facilityRepository;
            _specificationRepository = specificationRepository;
            _stockMovementRepository = stockMovementRepository;
            _stockMovementService = stockMovementService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Result<ProductionDto>> Handle(CreateOrUpdateProductionCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Проверяем существование производства
                var facility = await _facilityRepository.GetByIdAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new NotFoundException(nameof(Facility), request.FacilityId);
                }

                // Проверяем существование продукта
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    throw new NotFoundException(nameof(Product), request.ProductId);
                }

                // Проверяем, что продукт не является сырьем
                if (product.ProductType == ProductTypeEnum.RawMaterial)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<ProductionDto>("Нельзя производить продукт типа 'Сырье'");
                }

                // Получаем активную спецификацию продукта
                var activeSpecification = await _specificationRepository.GetQueryable()
                    .Include(ps => ps.Items)
                    .ThenInclude(psi => psi.Product)
                    .FirstOrDefaultAsync(ps => ps.ProductId == request.ProductId && ps.IsActive, cancellationToken);

                if (activeSpecification == null || !activeSpecification.Items.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<ProductionDto>($"Для продукта '{product.Name}' не найдена активная спецификация");
                }

                Production? production = null;
                var productionId = request.Id;
                if (productionId.HasValue)
                {
                    // Проверяем существование производства
                    production = await _productionRepository.GetByIdAsync(productionId.Value);
                    if (production == null)
                    {
                        throw new NotFoundException(nameof(Production), productionId.Value);
                    }

                    // Удаляем движения запасов
                    await _stockMovementService.DeleteStockMovements(productionId.Value, StockMovemetReferenceTypeEnum.Production);

                    // Пересчет балансов
                    await _stockMovementService.RecalculateAllBalancesFromDate(production.DateCreated.Date, production.FacilityId, production.ProductId);
                }

                // Проверяем достаточность остатков компонентов
                var componentValidationResult = await ValidateComponentAvailability(
                    activeSpecification.Items.ToList(), request.Quantity, request.FacilityId, cancellationToken);

                if (!componentValidationResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                    return Result.Failure<ProductionDto>(componentValidationResult.ErrorMessage);
                }

                if (production is null)
                {
                    // Создаем запись о производстве
                    production = new Production(request.ProductId, request.FacilityId, request.Quantity);
                    await _productionRepository.AddAsync(production);
                }
                else
                {
                    // Обновляем запись о производстве
                    production.UpdateEntity(
                        request.ProductId,
                        request.FacilityId,
                        request.Quantity
                    );

                    _productionRepository.Update(production);
                }

                // Создаем движения запасов для списания компонентов
                await CreateComponentStockMovements(activeSpecification.Items.ToList(),
                    request.Quantity, request.FacilityId, production.Id, production.DateCreated, cancellationToken);

                // Создаем движение запасов для прихода произведенного продукта
                await CreateProductStockMovement(product, request.Quantity, request.FacilityId, production.Id, production.DateCreated, cancellationToken);

                if (productionId.HasValue)
                {
                    await _stockMovementService.UpdateStockMovementsAndRecalculateBalances(
                        new UpdateStockMovementsAndRecalculateBalancesModel(
                            production.Id,
                            OperationTypeEnum.Income,
                            StockMovemetReferenceTypeEnum.Supply,
                            production.DateCreated,
                            production.FacilityId,
                            production.ProductId,
                            request.Quantity
                        )
                    );
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                // Получаем полные данные для ответа
                return await _mediator.Send(new GetProductionByIdQuery(production.Id), cancellationToken);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return Result.Failure<ProductionDto>($"Ошибка при создании производства: {ex.Message}", ex);
            }
        }

        private async Task<Result> ValidateComponentAvailability(
            List<ProductSpecificationItem> specificationItems,
            decimal productionQuantity,
            int facilityId,
            CancellationToken cancellationToken)
        {
            foreach (var item in specificationItems)
            {
                // Получаем текущий остаток компонента на производстве
                var currentBalanceResult = await _stockMovementService.GetStockBalanceByDate(facilityId, item.ProductId, DateTime.UtcNow.Date);
                var currentBalance = currentBalanceResult.FinishBalance;
                var requiredQuantity = item.Quantity * productionQuantity;

                if (currentBalance < requiredQuantity)
                {
                    return Result.Failure(
                        $"Недостаточно компонента '{item.Product.Name}'. " +
                        $"Требуется: {requiredQuantity}, доступно: {currentBalance}");
                }
            }

            return Result.Success();
        }

        private async Task CreateComponentStockMovements(
            List<ProductSpecificationItem> specificationItems,
            decimal productionQuantity,
            int facilityId,
            Guid productionId,
            DateTime date,
            CancellationToken cancellationToken)
        {
            foreach (var item in specificationItems)
            {
                var movementQuantity = item.Quantity * productionQuantity;

                var movement = new StockMovement(
                    facilityId: facilityId,
                    productId: item.ProductId,
                    operationType: OperationTypeEnum.Outcome,
                    referenceType: StockMovemetReferenceTypeEnum.Production,
                    referenceId: productionId.ToString(),
                    quantity: movementQuantity,
                    dateCreated: date);

                await _stockMovementRepository.AddAsync(movement);
            }
        }

        private async Task CreateProductStockMovement(
            Product product,
            decimal quantity,
            int facilityId,
            Guid productionId,
            DateTime date,
            CancellationToken cancellationToken)
        {
            var movement = new StockMovement(
                facilityId: facilityId,
                productId: product.Id,
                operationType: OperationTypeEnum.Income,
                referenceType: StockMovemetReferenceTypeEnum.Production,
                referenceId: productionId.ToString(),
                quantity: quantity,
                dateCreated: date);

            await _stockMovementRepository.AddAsync(movement);
        }
    }
}