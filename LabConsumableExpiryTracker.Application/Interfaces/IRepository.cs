namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IRepository<TEntity, in TKey>
    where TEntity : class
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken ct = default);

    Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken ct = default);

    Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken ct = default);

    Task<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken ct = default);

    Task<bool> DeleteAsync(
        TKey id,
        CancellationToken ct = default);
}