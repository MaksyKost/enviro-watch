namespace EnviroWatch.Application.DTOs;

public record CurrentWeatherData(
    DateTime Timestamp,
    double TemperatureCelsius,
    double HumidityPercent,
    double WindSpeedKmh);
