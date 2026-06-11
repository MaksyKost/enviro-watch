using EnviroWatch.Application.BackgroundServices;
using EnviroWatch.Application.Configuration;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Application.Services;
using EnviroWatch.Application.Validators;
using FluentValidation;
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
        services.Configure<AlertOptions>(
            configuration.GetSection(AlertOptions.SectionName));
        services.Configure<CleanupOptions>(
            configuration.GetSection(CleanupOptions.SectionName));
        services.AddOptions<OpenWeatherOptions>()
            .Bind(configuration.GetSection(OpenWeatherOptions.SectionName))
            .PostConfigure(options =>
            {
                var apiKey = configuration["OPENWEATHER_API_KEY"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    options.ApiKey = apiKey;
                }
            });

        services.AddScoped<IDataSnapshotService, DataSnapshotService>();
        services.AddScoped<IDataFetchService, DataFetchService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAlertCheckerService, AlertCheckerService>();
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<IOpenWeatherService, OpenWeatherService>();
        services.AddScoped<IAirQualityService, AirQualityService>();
        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<IObservationService, ObservationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddHostedService<DataFetcherBackgroundService>();
        services.AddHostedService<AlertProcessorBackgroundService>();
        services.AddHostedService<SnapshotCleanupBackgroundService>();

        return services;
    }
}
