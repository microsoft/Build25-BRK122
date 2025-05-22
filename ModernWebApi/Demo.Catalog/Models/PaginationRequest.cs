using Microsoft.AspNetCore.Mvc;

namespace Catalog.Models;

/// <summary>
/// Represents a request for paginated results.
/// </summary>
public class PaginationRequest(
    int pageSize = 10,
    int pageIndex = 0
)
{
    /// <summary>
    /// Number of items to include per page.
    /// </summary>
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = pageSize;

    /// <summary>
    /// Zero-based index of the page to retrieve.
    /// </summary>
    [FromQuery(Name = "pageIndex")]
    public int PageIndex { get; set; } = pageIndex;
}
