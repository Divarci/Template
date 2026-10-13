using Core.Library.Abstractions;
using Core.Library.ResultPattern;
using System.Net;

namespace Core.Domain.Entities.System.AuditLogs;

public sealed partial class AuditLog : BaseEntity
{
    private AuditLog() { }

    private AuditLog(string description, Guid entityId, DateTime createdOnUtc)
    {
        Id = Guid.NewGuid();
        Description = description;
        EntityId = entityId;
        CreatedOnUtc = createdOnUtc;
    }

    public string Description { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }

    public static Result<AuditLog> Create(Guid entityId, string description, DateTime createdOnUtc)
    {
        AuditLog auditLog = new(description, entityId, createdOnUtc);

        Result validationResult = Validate(auditLog);

        if (validationResult.IsFailure)
            return Result<AuditLog>.Failure(
                message: validationResult.Message,
                errorDetails: validationResult.ErrorDetails!,
                statusCode: validationResult.StatusCode);

        return Result<AuditLog>.Success(
            message: "Audit log created successfully",
            data: auditLog,
            statusCode: HttpStatusCode.Created);
    }
}
