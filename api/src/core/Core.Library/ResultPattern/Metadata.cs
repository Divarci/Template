using Core.Library.Exceptions;
using System.Text.Json.Serialization;

namespace Core.Library.ResultPattern;

public class Metadata
{
    [JsonConstructor]
    public Metadata(int pageNumber, int pageSize, int totalCount, int pageCount)
    {
        if (pageSize <= 0)
            throw new TemplateException(
                assemblyName: "Core.Library",
                className: nameof(Metadata),
                methodName: ".ctor",
                message: "PageSize must be greater than zero.");

        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        PageCount = pageCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int PageCount { get; set; }
    public int TotalPages { get; set; }
}
