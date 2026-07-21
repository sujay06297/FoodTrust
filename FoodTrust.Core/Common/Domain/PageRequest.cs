namespace FoodTrust.Core.Common.Domain;

public readonly record struct PageRequest
{
    private PageRequest(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }

    public int PageSize { get; }

    public static PageRequest Create(int page, int pageSize, int maxPageSize = 200)
    {
        return new PageRequest(
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, maxPageSize));
    }
}
