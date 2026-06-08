using Microsoft.AspNetCore.SignalR;

namespace EnviroWatch.API.Hubs;

/// <summary>
/// Real-time dashboard updates. Clients receive <c>DataUpdate</c> events after each fetch cycle.
/// </summary>
public class DashboardHub : Hub
{
    public const string HubPath = "/hubs/dashboard";
    public const string DataUpdateEvent = "DataUpdate";

    /// <summary>
    /// Subscribe to updates for a single region (e.g. Wroclaw,PL).
    /// </summary>
    public Task SubscribeToRegion(string region) =>
        Groups.AddToGroupAsync(Context.ConnectionId, RegionGroup(region));

    /// <summary>
    /// Stop receiving region-specific updates.
    /// </summary>
    public Task UnsubscribeFromRegion(string region) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, RegionGroup(region));

    public static string RegionGroup(string region) => $"region:{region}";
}
