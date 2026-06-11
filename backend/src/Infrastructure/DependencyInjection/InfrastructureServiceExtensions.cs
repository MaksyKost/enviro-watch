using EnviroWatch.Application.Interfaces;
using EnviroWatch.Infrastructure;
using EnviroWatch.Infrastructure.Auth;
using EnviroWatch.Infrastructure.ExternalClients;
using EnviroWatch.Infrastructure.Notifications;
using EnviroWatch.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnviroWatch.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDataSnapshotRepository, DataSnapshotRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IObservationRepository, ObservationRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IAlertNotifier, LoggingAlertNotifier>();

        services.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient<IOpenWeatherClient, OpenWeatherClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient<IOpenAQClient, OpenAQClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openaq.org/v2/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddHttpClient<IOpenSkyClient, OpenSkyClient>(client =>
        {
            client.BaseAddress = new Uri("https://opensky-network.org/api/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
