using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Requests;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Responses;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;

namespace Adapter.Restful.Controllers.VersionOne.Samples.Notes;

internal static class NoteMapper
{
    public static NoteCreateModel ToCreateModel(CreateNoteRequest request)
    {
        return new NoteCreateModel
        {
            Title = request.Title!,
            Content = request.Content,
            IsPinned = request.IsPinned,
        };
    }

    public static NoteUpdateModel ToUpdateModel(UpdateNoteRequest request)
    {
        return new NoteUpdateModel
        {
            Title = request.Title,
            Content = request.Content,
            IsPinned = request.IsPinned,
        };
    }

    public static NoteResponse ToResponse(Note note)
    {
        return new NoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            IsPinned = note.IsPinned,
        };
    }

    public static NoteResponse[] ToArrayResponse(Note[] notes)
    {
        return [.. notes.Select(ToResponse)];
    }
}
