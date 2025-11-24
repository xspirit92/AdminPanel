using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Products.Queries
{
    public class GetProductListQuery : BaseQuery, IRequest<Result<List<ProductDto>>>
    {
        public string? Name { get; set; }
        public ProductTypeEnum ProductType { get; set; } = ProductTypeEnum.RawMaterial;
        public UnitOfMeasureEnum? UnitOfMeasure { get; set; }

        protected override string DefaultSortBy => "name";

    }

    // Validator
    public class GetProductListQueryValidator : AbstractValidator<GetProductListQuery>
    {
        public GetProductListQueryValidator()
        {
            RuleFor(x => x.ProductType).NotNull().IsInEnum();
            RuleFor(x => x.UnitOfMeasure).IsInEnum().When(x => x.UnitOfMeasure.HasValue);
        }
    }
}
