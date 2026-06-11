namespace EnviroWatch.Application.DTOs;

public record FlightData(
    DateTime Timestamp,
    int AircraftCount,
    double? AverageAltitudeMeters);
