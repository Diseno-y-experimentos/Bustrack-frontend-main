namespace BusTrackBackEnd.API.IAM.Interfaces.REST.Resources;

public record CreateTravelHistoryResource(
    string Title,
    int? DurationMinutes,
    double? DistanceKm,
    object? Route
);

