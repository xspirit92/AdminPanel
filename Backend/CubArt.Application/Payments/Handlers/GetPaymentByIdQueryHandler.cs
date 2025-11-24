using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Payments.DTOs;
using CubArt.Application.Payments.Queries;
using CubArt.Application.Supplies.Queries;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Payments.Handlers
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public GetPaymentByIdQueryHandler(
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(request.Id);
                if (payment is null)
                {
                    throw new NotFoundException(nameof(payment), request.Id);
                }

                return Result.Success(_mapper.Map<PaymentDto>(payment));
            }
            catch (Exception ex)
            {
                return Result.Failure<PaymentDto>($"Ошибка получения оплаты: {ex.Message}", ex);
            }
        }
    }
}
