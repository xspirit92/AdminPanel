using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.Commands;
using CubArt.Application.Purchases.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Purchases.Handlers
{
    public class CreateOrUpdatePurchaseCommandHandler : IRequestHandler<CreateOrUpdatePurchaseCommand, Result<PurchaseDto>>
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Supplier, int> _suppliertRepository;
        private readonly IRepository<Facility, int> _facilityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateOrUpdatePurchaseCommandHandler(
            IPurchaseRepository purchaseRepository,
            IRepository<Product, int> productRepository,
            IRepository<Supplier, int> suppliertRepository,
            IRepository<Facility, int> facilityRepository,
            IUnitOfWork unitOfWork,
            IMediator mediator)
        {
            _purchaseRepository = purchaseRepository;
            _productRepository = productRepository;
            _suppliertRepository = suppliertRepository;
            _facilityRepository = facilityRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Result<PurchaseDto>> Handle(CreateOrUpdatePurchaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем существование сырья
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    throw new NotFoundException(nameof(Product), request.ProductId);
                }

                if (product.ProductType != ProductTypeEnum.RawMaterial)
                {
                    return Result.Failure<PurchaseDto>($"Закупки доступны только для сырья");
                }

                // Проверяем существование поставщика
                var supplier = await _suppliertRepository.GetByIdAsync(request.SupplierId);
                if (supplier == null)
                {
                    throw new NotFoundException(nameof(Supplier), request.SupplierId);
                }
                // Проверяем существование производства
                var facility = await _facilityRepository.GetByIdAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new NotFoundException(nameof(Facility), request.FacilityId);
                }

                Guid? purchaseId;
                if (request.Id is null)
                {
                    // Создание заказа
                    var purchase = new Purchase(
                        request.ProductId,
                        request.SupplierId,
                        request.FacilityId,
                        request.Amount,
                        request.Quantity);

                    await _purchaseRepository.AddAsync(purchase);
                    purchaseId = purchase.Id;
                }
                else
                {
                    // Обновление заказа
                    var purchase = await _purchaseRepository.GetByIdAsync(request.Id.Value);
                    if (purchase == null)
                    {
                        throw new NotFoundException(nameof(Purchase), request.Id.Value);
                    }

                    if (purchase.PurchaseStatus == PurchaseStatusEnum.Completed)
                    {
                        return Result.Failure<PurchaseDto>($"Ошибка при сохранении закупки: Закупку в статусе 'Завершена' запрещено редактировать");
                    }

                    purchase.UpdateEntity(
                        request.ProductId,
                        request.SupplierId,
                        request.FacilityId,
                        request.Amount,
                        request.Quantity);

                    _purchaseRepository.Update(purchase);
                    purchaseId = purchase.Id;
                }
                await _unitOfWork.CommitAsync(cancellationToken);

                return await _mediator.Send(new GetPurchaseByIdQuery(purchaseId.Value), cancellationToken);
            }
            catch (DomainException ex)
            {
                return Result.Failure<PurchaseDto>($"Ошибка при сохранении закупки: {ex.Message}");
            }
        }
    }
}
