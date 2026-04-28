namespace Maono.Domain.Common;

/// <summary>
/// Unit of Work — core domain contract.
/// Ensures atomic persistence of all changes within a use case.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Persists all tracked changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins an explicit database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
