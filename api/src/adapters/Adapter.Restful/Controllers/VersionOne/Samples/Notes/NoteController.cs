using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Requests;
using Adapter.Restful.Controllers.VersionOne.Samples.Notes.Models.Responses;
using Asp.Versioning;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Interfaces;
using Core.Library.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adapter.Restful.Controllers.VersionOne.Samples.Notes;

[ApiVersion(ApiVersion.V1)]
[Route("api/v{version:apiVersion}/notes")]
[ApiController]
[Authorize]
public class NoteController(
    INoteUseCase noteUseCase) : BaseController
{
    private readonly INoteUseCase _noteUseCase = noteUseCase;

    [HttpPost]
    [ProducesResponseType<Result<NoteResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateNoteAsync(
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        Result<Note> createResult = await _noteUseCase
            .CreateAsync(
                NoteMapper.ToCreateModel(request),
                cancellationToken);

        return HandleResult(createResult, NoteMapper.ToResponse);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateNoteAsync(
        Guid id,
        [FromBody] UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        Result updateResult = await _noteUseCase
            .UpdateAsync(
                id,
                NoteMapper.ToUpdateModel(request),
                cancellationToken);

        return HandleResult(updateResult);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteNoteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result deleteResult = await _noteUseCase
            .DeleteAsync(id, cancellationToken);

        return HandleResult(deleteResult);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Result<NoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNoteByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<NoteResponse> noteResult = await _noteUseCase
            .GetByIdAsync(
                id,
                NoteMapper.ToResponse,
                cancellationToken);

        return HandleResult(noteResult);
    }

    [HttpGet]
    [ProducesResponseType<PaginatedResult<NoteResponse[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetNotesAsync(
        [FromQuery] NoteFilterModel filter,
        CancellationToken cancellationToken)
    {
        PaginatedResult<NoteResponse[]> paginatedNotes = await _noteUseCase
            .GetPaginatedAsync(
                filter.Page,
                filter.PageSize,
                filter.SortBy,
                filter.IsAscending,
                filter.Keyword,
                filter.IsPinned,
                NoteMapper.ToArrayResponse,
                cancellationToken);

        return HandleResult(paginatedNotes);
    }
}
