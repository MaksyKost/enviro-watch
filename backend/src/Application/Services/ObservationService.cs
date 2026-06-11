using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Services;

public class ObservationService : IObservationService
{
    public const string SourceName = "manual";

    private readonly IObservationRepository _observationRepository;
    private readonly IDataSnapshotRepository _snapshotRepository;

    public ObservationService(
        IObservationRepository observationRepository,
        IDataSnapshotRepository snapshotRepository)
    {
        _observationRepository = observationRepository;
        _snapshotRepository = snapshotRepository;
    }

    public async Task<ObservationDto> CreateAsync(
        Guid userId,
        CreateObservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var observedAt = request.ObservedAt.HasValue
            ? SnapshotFactory.NormalizeTimestamp(request.ObservedAt.Value)
            : DateTime.UtcNow;

        var observation = new ManualObservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Region = request.Region.Trim(),
            Metric = request.Metric.Trim().ToLowerInvariant(),
            Value = request.Value,
            Unit = request.Unit,
            Lat = request.Lat,
            Lon = request.Lon,
            Notes = request.Notes,
            ObservedAt = observedAt,
            CreatedAt = DateTime.UtcNow
        };

        await _observationRepository.CreateAsync(observation, cancellationToken);

        await _snapshotRepository.AddRangeAsync(
        [
            new DataSnapshot
            {
                Id = Guid.NewGuid(),
                Source = SourceName,
                Region = observation.Region,
                Metric = observation.Metric,
                Value = Math.Round(observation.Value, 1),
                Unit = observation.Unit,
                Lat = observation.Lat,
                Lon = observation.Lon,
                Timestamp = observation.ObservedAt
            }
        ],
            cancellationToken);

        return MapObservation(observation);
    }

    public async Task<IReadOnlyList<ObservationDto>> GetObservationsAsync(
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var observations = isAdmin
            ? await _observationRepository.GetAllAsync(cancellationToken)
            : await _observationRepository.GetByUserIdAsync(userId, cancellationToken);

        return observations.Select(MapObservation).ToList();
    }

    private static ObservationDto MapObservation(ManualObservation observation) =>
        new(
            observation.Id,
            observation.Region,
            observation.Metric,
            observation.Value,
            observation.Unit,
            observation.Lat,
            observation.Lon,
            observation.Notes,
            observation.ObservedAt,
            observation.CreatedAt);
}
