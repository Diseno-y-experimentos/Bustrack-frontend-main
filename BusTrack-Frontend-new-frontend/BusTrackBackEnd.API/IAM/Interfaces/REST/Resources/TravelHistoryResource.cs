namespace BusTrackBackEnd.API.IAM.Interfaces.REST.Resources;

public record TravelHistoryResource(
    Guid Id,
    string Title,
    DateTime OccurredAt,
    int DurationMinutes,
    double DistanceKm,
    object? Route,
    DateTime CreatedAt
);

