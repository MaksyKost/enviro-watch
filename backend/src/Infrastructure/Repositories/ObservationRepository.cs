using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviroWatch.Infrastructure.Repositories;

public class ObservationRepository : IObservationRepository
{
    private readonly AppDbContext _db;

    public ObservationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ManualObservation> CreateAsync(
        ManualObservation observation,
        CancellationToken cancellationToken = default)
    {
        _db.ManualObservations.Add(observation);
        await _db.SaveChangesAsync(cancellationToken);
        return observation;
    }

    public async Task<IReadOnlyList<ManualObservation>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _db.ManualObservations.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.ObservedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ManualObservation>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _db.ManualObservations.AsNoTracking()
            .OrderByDescending(o => o.ObservedAt)
            .ToListAsync(cancellationToken);
}
