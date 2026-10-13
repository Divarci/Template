using System.Text.Json.Serialization;

namespace Adapter.Restful.Controllers.VersionOne.System.AuditLogs.Models.Responses;

public record AuditLogResponse
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("entityId")]
    public required Guid EntityId { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("createdOnUtc")]
    public required DateTime CreatedOnUtc { get; init; }
}
