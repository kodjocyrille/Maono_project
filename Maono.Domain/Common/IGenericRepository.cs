using System.Linq.Expressions;

namespace Maono.Domain.Common;

/// <summary>
/// Generic repository for entities that don't need their own dedicated repository.
/// Used by ECR-011 (ContentDependency) and ECR-017 (ContentAnnotation).
/// </summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
