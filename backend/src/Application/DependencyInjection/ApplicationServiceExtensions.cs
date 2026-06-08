using EnviroWatch.Application.BackgroundServices;
using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnviroWatch.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DataFetchOptions>(
            configuration.GetSection(DataFetchOptions.SectionName));

        services.AddScoped<IDataSnapshotService, DataSnapshotService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddHostedService<DataFetcherBackgroundService>();

        return services;
    }
}
