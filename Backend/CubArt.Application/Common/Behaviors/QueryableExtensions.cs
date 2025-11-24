using CubArt.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Application.Common.Behaviors
{
    public static class QueryableExtensions
    {
        public static async Task<PagedListDto<T>> ToPagedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedListDto<T>.Create(items, totalCount, pageNumber, pageSize);
        }

        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            string sortBy,
            bool sortDescending,
            Dictionary<string, Func<IQueryable<T>, IQueryable<T>>> sortMap)
        {
            if (sortMap.TryGetValue(sortBy.ToLower(), out var sortFunc))
            {
                return sortDescending
                    ? ReverseSort(query, sortFunc)
                    : sortFunc(query);
            }

            return query;
        }

        private static IQueryable<T> ReverseSort<T>(IQueryable<T> query, Func<IQueryable<T>, IQueryable<T>> sortFunc)
        {
            return sortFunc(query).Reverse();
        }
    }

}
