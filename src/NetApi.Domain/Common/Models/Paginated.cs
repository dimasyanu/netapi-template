namespace NetApi.Domain.Common.Models;

public class Paginated<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public long Total { get; set; }
    public long StartIndex { get; set; }
    public int PageSize { get; set; }

    public Paginated<TItems> CastItems<TItems>(Func<T, TItems> converter) where TItems : T
    {
        return new Paginated<TItems> {
            Items = [.. Items.Select(converter)],
            Total = Total,
            StartIndex = StartIndex,
            PageSize = PageSize
        };
    }
}
