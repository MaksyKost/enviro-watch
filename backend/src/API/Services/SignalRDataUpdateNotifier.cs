using EnviroWatch.API.Hubs;
using EnviroWatch.Application.DTOs;
using EnviroWatch.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EnviroWatch.API.Services;

public class SignalRDataUpdateNotifier : IDataUpdateNotifier
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<SignalRDataUpdateNotifier> _logger;

    public SignalRDataUpdateNotifier(
        IHubContext<DashboardHub> hubContext,
        ILogger<SignalRDataUpdateNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyDataUpdatesAsync(
        IReadOnlyList<DataUpdateDto> updates,
        CancellationToken cancellationToken = default)
    {
        foreach (var update in updates)
        {
            await _hubContext.Clients
                .All
                .SendAsync(DashboardHub.DataUpdateEvent, update, cancellationToken);

            _logger.LogDebug(
                "Pushed DataUpdate for region {Region} at {Timestamp}",
                update.Region,
                update.Timestamp);
        }
    }
}
