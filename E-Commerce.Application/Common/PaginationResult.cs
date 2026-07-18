namespace ECommerce.Application.Common;

public class PaginationResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    public PaginationResult(int pageNumber, int pageSize, int totalCount, IEnumerable<T> data)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        Data = data;
    }
}
