namespace Catalog.Models;

public class PaginatedItems<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data) where TEntity : class
{
    // Zero-based index of current page
    public int PageIndex { get; } = pageIndex;

    // Number of items per page
    public int PageSize { get; } = pageSize;

    // Total number of items across all pages
    public long Count { get; } = count;

    // Collection of items for the current page
    public IEnumerable<TEntity> Data { get;} = data;
}