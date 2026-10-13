using Adapter.Restful.Controllers.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models;

public record AuditLogFilterModel : PaginationFilterModel
{
    [FromQuery(Name = "entityId")]
    public Guid? EntityId { get; init; }

    [FromQuery(Name = "keyword")]
    public string? Keyword { get; init; }

    [FromQuery(Name = "fromDate")]
    public DateTime? FromDate { get; init; }

    [FromQuery(Name = "toDate")]
    public DateTime? ToDate { get; init; }
}
