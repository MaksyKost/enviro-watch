namespace EnviroWatch.Application.DTOs;

public record DataUpdateDto(
    string Type,
    string Region,
    WeatherMetricsDto Data,
    DateTime Timestamp);

public record WeatherMetricsDto(
    double Temperature,
    double Humidity,
    double Wind);
