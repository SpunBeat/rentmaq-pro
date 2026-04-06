using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly RentMaqDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(RentMaqDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(T entity)
        => DbSet.Update(entity);

    public void Remove(T entity)
        => DbSet.Remove(entity);
}
