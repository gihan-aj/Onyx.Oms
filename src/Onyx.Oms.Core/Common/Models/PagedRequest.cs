namespace Onyx.Oms.Core.Common.Models;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortColumn { get; init; }
    public string? SortOrder { get; init; } // "asc" or "desc"
}
