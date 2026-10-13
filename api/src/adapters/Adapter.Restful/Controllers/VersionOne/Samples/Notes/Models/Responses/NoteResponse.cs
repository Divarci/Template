using System.Text.Json.Serialization;

namespace Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Responses;

public record NoteResponse
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("content")]
    public required string? Content { get; init; }

    [JsonPropertyName("isPinned")]
    public required bool IsPinned { get; init; }
}
