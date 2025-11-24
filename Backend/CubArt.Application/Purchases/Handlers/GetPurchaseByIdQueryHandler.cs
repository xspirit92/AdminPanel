using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.Commands;
using CubArt.Application.Purchases.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Purchases.Handlers
{
    public class GetPurchaseByIdQueryHandler : IRequestHandler<GetPurchaseByIdQuery, Result<PurchaseDto>>
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IMapper _mapper;

        public GetPurchaseByIdQueryHandler(
            IPurchaseRepository purchaseRepository,
            IMapper mapper)
        {
            _purchaseRepository = purchaseRepository;
            _mapper = mapper;
        }

        public async Task<Result<PurchaseDto>> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var purchase = await _purchaseRepository.GetByIdAsync(request.Id, includeRelated: true);
                if (purchase is null)
                {
                    throw new NotFoundException(nameof(Purchase), request.Id);
                }

                return Result.Success(_mapper.Map<PurchaseDto>(purchase));
            }
            catch (Exception ex)
            {
                return Result.Failure<PurchaseDto>($"Ошибка получения закупки: {ex.Message}", ex);
            }
        }
    }
}
