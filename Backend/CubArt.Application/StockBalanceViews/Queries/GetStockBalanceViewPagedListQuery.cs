using CubArt.Application.Common.Models;
using CubArt.Application.StockBalances.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.StockBalances.Queries
{
    public class GetStockBalanceViewPagedListQuery : BasePagedFilterQuery, IRequest<Result<PagedListDto<StockBalanceViewDto>>>
    {
        public int? FacilityId { get; set; }
        public int? ProductId { get; set; }
        public ProductTypeEnum? ProductType { get; set; }
        public DateTime BalanceDate { get; set; }
        protected override string DefaultSortBy => "productname";

        public override void Normalize()
        {
            base.Normalize();

            // Если дата не указана, используем текущую дату
            if (BalanceDate == default)
            {
                BalanceDate = DateTime.UtcNow.Date;
            }
            else
            {
                BalanceDate = BalanceDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(BalanceDate, DateTimeKind.Utc)
                    : BalanceDate.ToUniversalTime();
            }
        }

    }

    // Validator
    public class GetStockBalanceViewPagedListQueryValidator : AbstractValidator<GetStockBalanceViewPagedListQuery>
    {
        public GetStockBalanceViewPagedListQueryValidator()
        {
            RuleFor(x => x.ProductType).IsInEnum().When(x => x.ProductType.HasValue);
        }
    }

}
