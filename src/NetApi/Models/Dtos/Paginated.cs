namespace NetApi.Models.Dtos;

public class Paginated<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int StartIndex { get; set; } 
}
