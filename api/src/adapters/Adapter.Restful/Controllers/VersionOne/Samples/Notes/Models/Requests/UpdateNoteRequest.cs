using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Requests;

public record UpdateNoteRequest
{
    [MaxLength(200)]
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [MaxLength(5000)]
    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("isPinned")]
    public bool? IsPinned { get; init; }
}
