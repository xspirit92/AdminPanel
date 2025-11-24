using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Products.Queries
{
    public class GetAllProductsQuery : BasePagedFilterQuery, IRequest<Result<PagedListDto<ProductDto>>>
    {
        public string? Name { get; set; }
        public ProductTypeEnum? ProductType { get; set; }
        public UnitOfMeasureEnum? UnitOfMeasure { get; set; }

        protected override string DefaultSortBy => "name";

    }

    // Validator
    public class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
    {
        public GetAllProductsQueryValidator()
        {
            RuleFor(x => x.ProductType).IsInEnum().When(x => x.ProductType.HasValue);
            RuleFor(x => x.UnitOfMeasure).IsInEnum().When(x => x.UnitOfMeasure.HasValue);
        }
    }
}
