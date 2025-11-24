using System.Text.Json.Serialization;

namespace CubArt.Application.Common.Models
{
    public class PagedListDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        [JsonConstructor]
        public PagedListDto(List<T> items, int totalCount, int pageNumber, int pageSize, int totalPages, bool hasPreviousPage, bool hasNextPage)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = totalPages;
            HasPreviousPage = hasPreviousPage;
            HasNextPage = hasNextPage;
        }

        public static PagedListDto<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedListDto<T>(
                items,
                totalCount,
                pageNumber,
                pageSize,
                totalPages,
                pageNumber > 1,
                pageNumber < totalPages
            );
        }
    }

}
