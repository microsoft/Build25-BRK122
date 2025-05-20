namespace Catalog.Models;

public class PaginationRequest
{
    // Number of items to include per page
    public int? PageSize { get; set; } = 10;

    // Zero-based index of the page to retrieve
    public int? PageIndex { get; set; } = 0;
}