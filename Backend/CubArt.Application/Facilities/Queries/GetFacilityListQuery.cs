using CubArt.Application.Common.Models;
using CubArt.Application.Facilities.DTOs;
using MediatR;

namespace CubArt.Application.Facilities.Queries
{
    public class GetFacilityListQuery : BaseQuery, IRequest<Result<List<FacilityDto>>>
    {
        public string? Name { get; set; }

        protected override string DefaultSortBy => "name";

    }
}
