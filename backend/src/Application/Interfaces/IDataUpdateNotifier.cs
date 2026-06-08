using EnviroWatch.Application.DTOs;

namespace EnviroWatch.Application.Interfaces;

public interface IDataUpdateNotifier
{
    Task NotifyDataUpdatesAsync(
        IReadOnlyList<DataUpdateDto> updates,
        CancellationToken cancellationToken = default);
}
