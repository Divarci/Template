using Core.Library.ResultPattern;

namespace Core.Library.Contracts.UnitOfWorks;

/// <summary>
/// Defines the contract for the Unit of Work pattern, coordinating the writing
/// of changes and managing database transactions across multiple repositories.
/// </summary>
public interface ITransactionalUnitOfWork
{
    /// <summary>
    /// Executes the given operation within a database transaction, committing on success
    /// and rolling back on failure.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The operation's result after the transaction commits when the operation succeeds;
    /// the operation's failure result after rollback when it fails; the commit failure result
    /// after rollback when the domain events cannot be converted to outbox messages; or, when an
    /// exception is thrown, a <c>500 Internal Server Error</c> failure result after rollback whose
    /// message carries a trace id and no exception detail.
    /// </returns>
    Task<Result> ExecuteTransactionAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the given operation within a database transaction, committing on success
    /// and rolling back on failure.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The operation's result after the transaction commits when the operation succeeds;
    /// the operation's failure result after rollback when it fails without data; the commit
    /// failure (its message and status code) after rollback when the domain events cannot be
    /// converted to outbox messages; or, when an exception is thrown, a <c>500 Internal Server Error</c>
    /// failure result after rollback whose message carries a trace id and no exception detail.
    /// </returns>
    Task<Result<T>> ExecuteTransactionAsync<T>(
        Func<Task<Result<T>>> operation,
        CancellationToken cancellationToken = default);
}
