namespace norviguet_control_fletes_api.Models.DTOs.Common
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }
        public int TotalPages =>
            (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
