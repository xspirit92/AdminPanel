using AutoMapper;
using CubArt.Application.Common.Models;
using CubArt.Application.Productions.DTOs;
using CubArt.Application.Productions.Queries;
using CubArt.Domain.Enums;
using CubArt.Domain.Exceptions;
using CubArt.Infrastructure.Interfaces;
using MediatR;

namespace CubArt.Application.Productions.Handlers
{
    public class GetProductionByIdQueryHandler : IRequestHandler<GetProductionByIdQuery, Result<ProductionDto>>
    {
        private readonly IProductionRepository _productionRepository;
        private readonly IStockMovementService _stockMovementService;
        private readonly IMapper _mapper;

        public GetProductionByIdQueryHandler(
            IProductionRepository productionRepository,
            IStockMovementService stockMovementService,
            IMapper mapper)
        {
            _productionRepository = productionRepository;
            _stockMovementService = stockMovementService;
            _mapper = mapper;
        }

        public async Task<Result<ProductionDto>> Handle(GetProductionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var production = await _productionRepository.GetByIdAsync(request.Id);
                if (production is null)
                {
                    throw new NotFoundException(nameof(production), request.Id);
                }

                var dto = _mapper.Map<ProductionDto>(production);
                var stockMovements = await _stockMovementService.GetStockMovementsByReference(production.Id.ToString(), StockMovemetReferenceTypeEnum.Production);
                dto.StockMovementList = _mapper.Map<IEnumerable<StockMovementDto>>(stockMovements);

                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProductionDto>($"Ошибка получения производства: {ex.Message}", ex);
            }
        }
    }
}
