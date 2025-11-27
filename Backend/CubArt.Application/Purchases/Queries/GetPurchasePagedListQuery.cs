using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Purchases.Commands
{
    public class GetPurchasePagedListQuery : BaseDateFilterQuery, IRequest<Result<PagedListDto<PurchaseDto>>>
    {
        public int? SupplierId { get; set; }
        public int? FacilityId { get; set; }
        public int? ProductId { get; set; }
        public PurchaseStatusEnum? PurchaseStatus { get; set; }

        protected override string DefaultSortBy => "datecreated";

    }

    // Validator
    public class GetPurchasePagedQueryValidator : AbstractValidator<GetPurchasePagedListQuery>
    {
        public GetPurchasePagedQueryValidator()
        {
            RuleFor(x => x.PurchaseStatus).IsInEnum().When(x => x.PurchaseStatus.HasValue);
        }
    }
}
