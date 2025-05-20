namespace Catalog.Models;

/// <summary>
/// Represents a paginated collection of items for use in API responses.
/// </summary>
/// <typeparam name="TEntity">The type of elements in the paginated collection.</typeparam>
public class PaginatedItems<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data) where TEntity : class
{
    /// <summary>
    /// Gets the zero-based index of current page.
    /// </summary>
    public int PageIndex { get; } = pageIndex;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public long Count { get; } = count;

    /// <summary>
    /// Collection of items for the current page
    /// </summary>
    public IEnumerable<TEntity> Data { get;} = data;
}
