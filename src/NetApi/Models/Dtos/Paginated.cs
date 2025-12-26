namespace NetApi.Models.Dtos;

public class Paginated<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public long TotalCount { get; set; }
    public int PageSize { get; set; }
    public long StartIndex { get; set; }
}
