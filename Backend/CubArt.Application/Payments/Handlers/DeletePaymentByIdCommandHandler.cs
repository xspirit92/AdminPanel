using CubArt.Application.Common.Models;
using CubArt.Application.Payments.Commands;
using CubArt.Application.Payments.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Payments.Handlers
{
    public class DeletePaymentByIdCommandHandler : IRequestHandler<DeletePaymentByIdCommand, Result>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePaymentByIdCommandHandler(
            IPaymentRepository paymentRepository,
            IPurchaseRepository purchaseRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeletePaymentByIdCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем существование оплаты
                var payment = await _paymentRepository.GetByIdAsync(request.Id);
                if (payment == null)
                {
                    throw new NotFoundException(nameof(Payment), request.Id);
                }

                // Проверяем закупку
                var purchase = await _purchaseRepository.GetByIdAsync(payment.PurchaseId);
                if (purchase == null)
                {
                    throw new NotFoundException(nameof(Purchase), payment.PurchaseId);
                }

                var deniedStatuses = new PurchaseStatusEnum[] { PurchaseStatusEnum.Completed, PurchaseStatusEnum.Paid };
                if (deniedStatuses.Contains(purchase.PurchaseStatus))
                {
                    return Result.Failure<PaymentDto>($"Ошибка при удалении оплаты: Нельзя редактировать оплаченную закупку");
                }

                _paymentRepository.Delete(payment);
                await _unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure<PaymentDto>($"Ошибка при удалении оплаты: {ex.Message}", ex);
            }
        }
    }

}
