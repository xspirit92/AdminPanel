using CubArt.Application.Common.Models;
using CubArt.Application.Payments.DTOs;
using CubArt.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CubArt.Application.Payments.Queries
{
    public class GetPaymentPagedListQuery : BaseDateFilterQuery, IRequest<Result<PagedListDto<PaymentDto>>>
    {
        public Guid? PurchaseId { get; set; }
        public PaymentMethodEnum? PaymentMethod { get; set; }
        public PaymentStatusEnum? PaymentStatus { get; set; }

        protected override string DefaultSortBy => "datecreated";

    }

    // Validator
    public class GetAllPaymentsQueryValidator : AbstractValidator<GetPaymentPagedListQuery>
    {
        public GetAllPaymentsQueryValidator()
        {
            RuleFor(x => x.PaymentMethod).IsInEnum().When(x => x.PaymentMethod.HasValue);
            RuleFor(x => x.PaymentStatus).IsInEnum().When(x => x.PaymentStatus.HasValue);
        }
    }
}
