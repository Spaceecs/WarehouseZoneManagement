using Microsoft.EntityFrameworkCore;

namespace Application.Core;

public class PagedList<T>(List<T> items, int count, int page, int pageSize)
{
    public List<T> Items { get; set; } = items;
    public int TotalCount { get; set; } = count;
    public int Page { get; set; } = page;
    public int PageSize { get; set; } = pageSize;

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int page, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedList<T>(items, count, page, pageSize);
    }
}
