using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.DTOs;
using CubArt.Application.Supplies.Queries;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;
using static StackExchange.Redis.Role;

namespace CubArt.Application.Supplies.Handlers
{
    public class GetSupplyByIdQueryHandler : IRequestHandler<GetSupplyByIdQuery, Result<SupplyDto>>
    {
        private readonly ISupplyRepository _supplyRepository;
        private readonly IStockMovementService _stockMovementService;
        private readonly IMapper _mapper;

        public GetSupplyByIdQueryHandler(
            ISupplyRepository supplyRepository,
            IStockMovementService stockMovementService,
            IMapper mapper)
        {
            _supplyRepository = supplyRepository;
            _stockMovementService = stockMovementService;
            _mapper = mapper;
        }

        public async Task<Result<SupplyDto>> Handle(GetSupplyByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var supply = await _supplyRepository.GetByIdAsync(request.Id);
                if (supply is null)
                {
                    throw new NotFoundException(nameof(supply), request.Id);
                }

                var dto = _mapper.Map<SupplyDto>(supply);
                var stockMovements = await _stockMovementService.GetStockMovementsByReference(supply.Id.ToString(), StockMovemetReferenceTypeEnum.Supply);
                dto.StockMovementList = _mapper.Map<IEnumerable<StockMovementDto>>(stockMovements);

                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                return Result.Failure<SupplyDto>($"Ошибка получения поставки: {ex.Message}", ex);
            }
        }
    }
}
