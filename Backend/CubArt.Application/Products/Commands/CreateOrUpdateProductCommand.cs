using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Products.Commands
{
    public class CreateOrUpdateProductCommand : IRequest<Result<ProductDto>>
    {
        public int? Id { get; set; } // null для создания, значение для обновления
        public string Name { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
        public ProductSpecificationData? Specification { get; set; }
    }

    public class ProductSpecificationData
    {
        public string? Version { get; set; }
        public bool SetAsActive { get; set; } = true;
        public List<SpecificationItem> Items { get; set; } = new();
    }

    public class SpecificationItem
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class CreateOrUpdateProductCommandValidator : AbstractValidator<CreateOrUpdateProductCommand>
    {
        public CreateOrUpdateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ProductType).IsInEnum();
            RuleFor(x => x.UnitOfMeasure).IsInEnum();

            When(x => x.Specification != null, () =>
            {
                RuleFor(x => x.Specification.Version).MaximumLength(50);
                RuleFor(x => x.Specification.Items).NotEmpty().When(x => x.Id.HasValue);
                RuleForEach(x => x.Specification.Items).ChildRules(item =>
                {
                    item.RuleFor(i => i.ProductId).GreaterThan(0);
                    item.RuleFor(i => i.Quantity).GreaterThan(0);
                });
            });
        }
    }

}
