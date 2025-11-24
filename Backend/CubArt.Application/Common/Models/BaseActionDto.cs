namespace CubArt.Application.Common.Models
{
    public abstract class BaseActionDto
    {
        public IEnumerable<StockMovementDto> StockMovementList { get; set; }
    }
}
