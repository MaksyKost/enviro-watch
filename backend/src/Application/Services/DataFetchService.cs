using EnviroWatch.Application.Interfaces;
using EnviroWatch.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EnviroWatch.Application.Services;

public class DataFetchService : IDataFetchService
{
    private readonly IWeatherService _weatherService;
    private readonly IOpenWeatherService _openWeatherService;
    private readonly IAirQualityService _airQualityService;
    private readonly IFlightService _flightService;
    private readonly ILogger<DataFetchService> _logger;

    public DataFetchService(
        IWeatherService weatherService,
        IOpenWeatherService openWeatherService,
        IAirQualityService airQualityService,
        IFlightService flightService,
        ILogger<DataFetchService> logger)
    {
        _weatherService = weatherService;
        _openWeatherService = openWeatherService;
        _airQualityService = airQualityService;
        _flightService = flightService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DataSnapshot>> FetchAllSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        var meteoTask = _weatherService.FetchCurrentWeatherSnapshotsAsync(cancellationToken);
        var openWeatherTask = _openWeatherService.FetchCurrentWeatherSnapshotsAsync(cancellationToken);
        var airQualityTask = _airQualityService.FetchCurrentAirQualitySnapshotsAsync(cancellationToken);
        var flightTask = _flightService.FetchCurrentFlightSnapshotsAsync(cancellationToken);

        await Task.WhenAll(meteoTask, openWeatherTask, airQualityTask, flightTask);

        var meteo = await meteoTask;
        var openWeather = await openWeatherTask;
        var airQuality = await airQualityTask;
        var flights = await flightTask;

        var snapshots = meteo
            .Concat(openWeather)
            .Concat(airQuality)
            .Concat(flights)
            .ToList();

        _logger.LogInformation(
            "Fetched snapshots from all sources: openmeteo={Meteo}, openweather={OpenWeather}, openaq={OpenAq}, opensky={OpenSky}",
            meteo.Count,
            openWeather.Count,
            airQuality.Count,
            flights.Count);

        return snapshots;
    }
}
