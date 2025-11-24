using CubArt.Application.Common.Models;
using CubArt.Application.Suppliers.DTOs;
using MediatR;

namespace CubArt.Application.Suppliers.Queries
{
    public class GetSupplierListQuery : BaseQuery, IRequest<Result<List<SupplierDto>>>
    {
        public string? Name { get; set; }

        protected override string DefaultSortBy => "name";

    }
}
