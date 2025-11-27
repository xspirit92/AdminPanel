using CubArt.Application.Common.Models;
using CubArt.Application.Purchases.DTOs;
using MediatR;

namespace CubArt.Application.Purchases.Queries
{
    public class GetPurchaseListQuery : BaseQuery, IRequest<Result<List<PurchaseDto>>>
    {
        public string? Name { get; set; }

        protected override string DefaultSortBy => "datecreated";

    }
}
