using CubArt.Application.Common.Models;
using CubArt.Application.Productions.DTOs;
using MediatR;

namespace CubArt.Application.Productions.Queries
{
    public class GetAllProductionsQuery : BaseDateFilterQuery, IRequest<Result<PagedListDto<ProductionDto>>>
    {
        public int? ProductId { get; set; }
        public int? FacilityId { get; set; }

        protected override string DefaultSortBy => "datecreated";

    }
}
