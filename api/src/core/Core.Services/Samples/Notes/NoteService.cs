using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Domain.Entities.System.AuditLogs.Interfaces;
using Core.Library.Contracts.Caching;
using Microsoft.Extensions.Logging;

namespace Core.Services.Samples.Notes;

internal sealed partial class NoteService(
    INoteRepository noteRepository,
    IAuditLogRepository auditLogRepository,
    ICacheInvalidationService cacheInvalidationService,
    ILogger<NoteService> logger) : INoteService
{
    private readonly INoteRepository _noteRepository = noteRepository;
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository;
    private readonly ICacheInvalidationService _cacheInvalidationService = cacheInvalidationService;
    private readonly ILogger<NoteService> _logger = logger;
}
