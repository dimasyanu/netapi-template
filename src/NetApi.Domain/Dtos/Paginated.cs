namespace NetApi.Domain.Dtos;

public class Paginated<T>
{
    public List<T> Items { get; set; } = [];
    public long Total { get; set; }
    public long StartIndex { get; set; }
    public int PageSize { get; set; }
}
