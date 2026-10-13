using Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models;
using Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models.Responses;
using Asp.Versioning;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adapter.Restful.Controllers.VersionOne.System.AuditLogs;

[ApiVersion(ApiVersion.V1)]
[Route("api/v{version:apiVersion}/audit-logs")]
[ApiController]
[Authorize]
public class AuditLogController(
    IAuditLogUseCase auditLogUseCase) : BaseController
{
    private readonly IAuditLogUseCase _auditLogUseCase = auditLogUseCase;

    [HttpGet]
    [ProducesResponseType<PaginatedResult<AuditLogResponse[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLogsAsync(
        [FromQuery] AuditLogFilterModel filter,
        CancellationToken cancellationToken)
    {
        PaginatedResult<AuditLogResponse[]> paginatedAuditLogs = await _auditLogUseCase
            .GetPaginatedAsync(
                filter.EntityId,
                filter.Page,
                filter.PageSize,
                filter.SortBy,
                filter.IsAscending,
                filter.Keyword,
                filter.FromDate,
                filter.ToDate,
                AuditLogMapper.ToArrayResponse,
                cancellationToken);

        return HandleResult(paginatedAuditLogs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Result<AuditLogResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLogByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<AuditLogResponse> auditLogResult = await _auditLogUseCase
            .GetByIdAsync(
                id,
                AuditLogMapper.ToResponse,
                cancellationToken);

        return HandleResult(auditLogResult);
    }
}
