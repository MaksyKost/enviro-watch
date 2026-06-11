using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Interfaces;

public interface IAlertNotifier
{
    Task NotifyAsync(Alert alert, AlertLog log, CancellationToken cancellationToken = default);
}
