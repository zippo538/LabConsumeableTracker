namespace LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

public  class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "Page minimal 1.");
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize minimal 1.");
        if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount), "TotalCount tidak boleh negatif.");

        Items = items ?? Array.Empty<T>();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public PagedResult() { }
}
