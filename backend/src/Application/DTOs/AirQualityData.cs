namespace EnviroWatch.Application.DTOs;

public record AirQualityData(
    DateTime Timestamp,
    double? Pm25,
    double? Pm10,
    double? Aqi);
