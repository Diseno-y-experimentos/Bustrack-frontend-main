namespace BusTrackBackEnd.API.IAM.Domain.Model.Aggregates;

public class TravelHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // FK al usuario
    public int UserId { get; set; }
    
    // Datos visibles para la UI
    public string Title { get; set; } = "";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
    // Datos técnicos
    public int DurationMinutes { get; set; }
    public double DistanceKm { get; set; }
    
    // JSON serializado con la ruta completa
    public string RouteJson { get; set; } = "";
    
    // Campos de auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Relación
    public virtual User? User { get; set; }
    
    protected TravelHistory() {}
    
    public TravelHistory(int userId, string title, int durationMinutes, double distanceKm, string routeJson)
    {
        UserId = userId;
        Title = title;
        DurationMinutes = durationMinutes;
        DistanceKm = distanceKm;
        RouteJson = routeJson;
        OccurredAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}

