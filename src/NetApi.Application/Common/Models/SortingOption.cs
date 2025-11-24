using System.ComponentModel.DataAnnotations;

namespace NetApi.Application.Common.Models;

public class SortingOption
{
    public const string DIRECTION_ASCENDING = "asc";
    public const string DIRECTION_DESCENDING = "desc";

    public string? SortBy { get; set; }

    [AllowedValues(DIRECTION_ASCENDING, DIRECTION_DESCENDING)]
    public string? SortDirection { get; set; }
}