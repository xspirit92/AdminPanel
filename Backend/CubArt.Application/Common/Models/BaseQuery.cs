namespace CubArt.Application.Common.Models
{
    public abstract class BaseQuery
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        protected virtual string DefaultSortBy => "name";

        public virtual void Normalize()
        {
            SortBy ??= DefaultSortBy;
        }
    }

    public abstract class BasePagedFilterQuery : BaseQuery
    {
        public int PageNumber { get; set; } = MinPageSize;
        public int PageSize { get; set; } = DefaultPageSize;
        
        protected const int MinPageSize = 1;
        protected const int MaxPageSize = 1000;
        protected const int MinPageNumber = 1;
        protected const int MaxPageNumber = 1000;
        protected const int DefaultPageSize = 20;
        
        public override void Normalize()
        {
            base.Normalize();

            PageNumber = Math.Clamp(PageNumber, MinPageNumber, MaxPageNumber);
            PageSize = Math.Clamp(PageSize, MinPageSize, MaxPageSize); // Ограничиваем размер страницы
        }
    }

    public abstract class BaseDateFilterQuery : BasePagedFilterQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void Normalize()
        {
            base.Normalize();

            if (StartDate.HasValue)
            {
                StartDate = StartDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(StartDate.Value, DateTimeKind.Utc)
                : StartDate.Value.ToUniversalTime();
            }
            if (EndDate.HasValue)
            {
                EndDate = EndDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(EndDate.Value, DateTimeKind.Utc)
                : EndDate.Value.ToUniversalTime();
            }

            // Корректируем даты для правильного фильтра
            if (EndDate.HasValue)
            {
                EndDate = EndDate.Value.Date.AddDays(1).AddTicks(-1); // Конец дня
            }
        }
    }

}
