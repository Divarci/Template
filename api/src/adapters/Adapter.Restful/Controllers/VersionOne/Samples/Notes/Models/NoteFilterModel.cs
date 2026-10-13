using Adapter.Restful.Controllers.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models;

public record NoteFilterModel : PaginationFilterModel
{
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; init; }

    [FromQuery(Name = "isPinned")]
    public bool? IsPinned { get; init; }
}
