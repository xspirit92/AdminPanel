using CubArt.Application.Common.Models;
using CubArt.Application.Products.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Products.Commands
{
    public class CreateProductCommand : IRequest<Result<ProductDto>>
    {
        public string Name { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public UnitOfMeasureEnum UnitOfMeasure { get; set; }
    }

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ProductType).IsInEnum();
            RuleFor(x => x.UnitOfMeasure).IsInEnum();
        }
    }

}
