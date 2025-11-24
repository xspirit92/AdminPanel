using CubArt.Application.Common.Models;
using CubArt.Application.Payments.DTOs;
using CubArt.Application.Purchases.Commands;
using CubArt.Application.Purchases.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Purchases.Handlers
{
    public class DeletePurchaseByIdCommandHandler : IRequestHandler<DeletePurchaseByIdCommand, Result>
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePurchaseByIdCommandHandler(
            IPurchaseRepository purchaseRepository,
            IUnitOfWork unitOfWork)
        {
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeletePurchaseByIdCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var purchase = await _purchaseRepository.GetByIdAsync(request.Id);
                if (purchase == null)
                {
                    throw new NotFoundException(nameof(Purchase), request.Id);
                }

                var deniedStatuses = new PurchaseStatusEnum[] { PurchaseStatusEnum.Completed, PurchaseStatusEnum.Paid };
                if (deniedStatuses.Contains(purchase.PurchaseStatus))
                {
                    return Result.Failure<PaymentDto>($"Ошибка при удалении закупки: Данная закупка оплачена");
                }

                _purchaseRepository.Delete(purchase);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (DomainException ex)
            {
                return Result.Failure<PurchaseDto>($"Ошибка при удалении закупки: {ex.Message}");
            }
        }
    }
}
