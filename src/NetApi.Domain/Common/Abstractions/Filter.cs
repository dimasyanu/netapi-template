namespace NetApi.Domain.Common.Abstractions;

public abstract class Filter
{
    public string? SearchTerm { get; set; }
    public long StartIndex { get; set; }
    public int PageSize { get; set; }
}
