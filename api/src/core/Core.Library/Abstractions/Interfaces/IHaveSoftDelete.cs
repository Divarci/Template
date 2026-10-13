using Core.Library.ResultPattern;

namespace Core.Library.Abstractions.Interfaces;

/// <summary>
/// Marks an entity as soft-deletable. Instead of removing the record from the database,
/// soft-deleted entities are hidden from all standard queries by setting <see cref="IsDeleted"/> to <c>true</c>.
/// </summary>
public interface IHaveSoftDelete
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// Soft-deleted entities are excluded from all standard read queries.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Marks the entity as soft-deleted by setting <see cref="IsDeleted"/> to <c>true</c>.
    /// </summary>
    /// <returns>A result indicating whether the soft-delete operation succeeded.</returns>
    Result SoftDelete();
}
