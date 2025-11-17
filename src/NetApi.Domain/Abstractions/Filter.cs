namespace NetApi.Domain.Abstractions;

public abstract class Filter
{
    public long StartIndex { get; set; }
    public int PageSize { get; set; }
}
