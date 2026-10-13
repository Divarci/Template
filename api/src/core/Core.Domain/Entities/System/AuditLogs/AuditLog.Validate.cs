using Core.Library.ResultPattern;
using Core.Library.Validators;
using Core.Library.Validators.Enums;

namespace Core.Domain.Entities.System.AuditLogs;

public sealed partial class AuditLog
{
    private static Result Validate(AuditLog auditLog)
    {
        List<Result> results = [];

        ValidateProperties(results, auditLog);
        ValidateBusiness(results, auditLog);

        return Result.CombineValidationErrors(results);
    }

    private static void ValidateProperties(List<Result> results, AuditLog auditLog)
    {
        results.AddRange(
            [
                auditLog.EntityId.Validate(
                    fieldName: nameof(auditLog.EntityId),
                    allowEmpty: false),
                auditLog.Description.Validate(
                    fieldName: nameof(auditLog.Description),
                    maxLength: 500),
                auditLog.CreatedOnUtc.Validate(
                    fieldName: nameof(auditLog.CreatedOnUtc),
                    restriction: DateTimeRestriction.OnlyPast)
            ]
        );
    }

    private static void ValidateBusiness(List<Result> results, AuditLog auditLog)
    {
    }
}
