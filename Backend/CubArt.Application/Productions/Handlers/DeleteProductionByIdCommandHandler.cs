using CubArt.Application.Common.Models;
using CubArt.Application.Productions.Commands;
using CubArt.Application.Productions.DTOs;
using CubArt.Application.Productions.Queries;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Productions.Handlers
{
    public class DeleteProductionByIdCommandHandler : IRequestHandler<DeleteProductionByIdCommand, Result>
    {
        private readonly IProductionRepository _productionRepository;
        private readonly IStockMovementService _stockMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public DeleteProductionByIdCommandHandler(
            IProductionRepository productionRepository,
            IStockMovementService stockMovementService,
            IUnitOfWork unitOfWork,
            IMediator mediator)

        {
            _productionRepository = productionRepository;
            _stockMovementService = stockMovementService;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Result> Handle(DeleteProductionByIdCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {                
                // Проверяем существование производства
                var production = await _productionRepository.GetByIdAsync(request.Id);
                if (production == null)
                {
                    throw new NotFoundException(nameof(Production), request.Id);
                }

                _productionRepository.Delete(production);

                // Удаляем движения запасов
                await _stockMovementService.DeleteStockMovements(production.Id, StockMovemetReferenceTypeEnum.Production);

                // Пересчет балансов
                await _stockMovementService.RecalculateAllBalancesFromDate(production.DateCreated.Date, production.FacilityId, production.ProductId);
                
                await _unitOfWork.CommitAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
                return Result.Failure<ProductionDto>($"Ошибка при удалении производства: {ex.Message}", ex);
            }
        }
    }
}