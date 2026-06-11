namespace EnviroWatch.Application.Interfaces;

public interface IAlertCheckerService
{
    Task<int> ProcessActiveAlertsAsync(CancellationToken cancellationToken = default);
}
