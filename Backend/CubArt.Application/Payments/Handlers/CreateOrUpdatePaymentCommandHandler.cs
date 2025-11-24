using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Payments.Commands;
using CubArt.Application.Payments.DTOs;
using CubArt.Application.Purchases.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Payments.Handlers
{
    public class CreateOrUpdatePaymentCommandHandler : IRequestHandler<CreateOrUpdatePaymentCommand, Result<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateOrUpdatePaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IPurchaseRepository purchaseRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _purchaseRepository = purchaseRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaymentDto>> Handle(CreateOrUpdatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем существование закупки
                var purchase = await _purchaseRepository.GetByIdAsync(request.PurchaseId);
                if (purchase == null)
                {
                    throw new NotFoundException(nameof(Purchase), request.PurchaseId);
                }

                var deniedStatuses = new PurchaseStatusEnum[] { PurchaseStatusEnum.Completed, PurchaseStatusEnum.Paid };
                if (deniedStatuses.Contains(purchase.PurchaseStatus))
                {
                    return Result.Failure<PaymentDto>($"Ошибка при сохранении оплаты: Данная закупка оплачена");
                }

                var paymentId = request.Id;
                // Проверяем, не превышает ли оплата оставшуюся сумму
                var paidAmount = await _paymentRepository.GetTotalPaidAmountAsync(request.PurchaseId, paymentId);
                var remainingAmount = purchase.Amount - paidAmount;

                if (request.Amount > remainingAmount)
                {
                    return Result.Failure<PaymentDto>($"Сумма оплаты {request.Amount} превосходит оставшуюся сумму {remainingAmount}");
                }

                Payment? payment;
                if (paymentId is null)
                {
                    // Создаем оплату
                    payment = new Payment(
                        request.PurchaseId,
                        request.Amount,
                        request.PaymentMethod);

                    await _paymentRepository.AddAsync(payment);
                }
                else
                {
                    // Обновление оплаты
                    payment = await _paymentRepository.GetByIdAsync(paymentId.Value);
                    if (payment == null)
                    {
                        throw new NotFoundException(nameof(Payment), paymentId.Value);
                    }

                    payment.UpdateEntity(
                        request.PurchaseId,
                        request.Amount,
                        request.PaymentMethod);

                    _paymentRepository.Update(payment);
                }

                // Если оплата полная, то переводим статус закупки
                if (remainingAmount - request.Amount == 0)
                {
                    purchase.PurchaseStatus = PurchaseStatusEnum.Paid;
                    _purchaseRepository.Update(purchase);
                }

                await _unitOfWork.CommitAsync(cancellationToken);

                return Result.Success(_mapper.Map<PaymentDto>(payment));
            }
            catch (Exception ex)
            {
                return Result.Failure<PaymentDto>($"Ошибка при создании оплаты: {ex.Message}", ex);
            }
        }
    }

}
