using CubArt.Application.Common.Models;
using CubArt.Application.Supplies.DTOs;
using MediatR;

namespace CubArt.Application.Supplies.Queries
{
    public class GetSupplyPagedListQuery : BaseDateFilterQuery, IRequest<Result<PagedListDto<SupplyDto>>>
    {
        public Guid? PurchaseId { get; set; }

        protected override string DefaultSortBy => "datecreated";

    }
}
