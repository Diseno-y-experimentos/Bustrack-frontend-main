using System.Text.Json;
using BusTrackBackEnd.API.IAM.Application.Internal.OutboundServices;
using BusTrackBackEnd.API.IAM.Domain.Model.Aggregates;
using BusTrackBackEnd.API.IAM.Interfaces.REST.Resources;
using BusTrackBackEnd.API.Shared.Infrastructure.Persistence.EFC;
using Microsoft.EntityFrameworkCore;

namespace BusTrackBackEnd.API.IAM.Infrastructure.TravelHistory.Services;

public class TravelHistoryService : ITravelHistoryService
{
    private readonly AppDbContext _db;

    public TravelHistoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TravelHistoryResource> CreateAsync(int userId, CreateTravelHistoryResource createDto)
    {
        var routeJson = createDto.Route != null 
            ? JsonSerializer.Serialize(createDto.Route) 
            : "";

        var entity = new Domain.Model.Aggregates.TravelHistory(
            userId,
            createDto.Title,
            createDto.DurationMinutes ?? 0,
            createDto.DistanceKm ?? 0,
            routeJson
        );

        _db.TravelHistories.Add(entity);
        await _db.SaveChangesAsync();

        return MapToResource(entity);
    }

    public async Task<IEnumerable<TravelHistoryResource>> GetForUserAsync(int userId, int page = 1, int pageSize = 20)
    {
        var items = await _db.TravelHistories
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(MapToResource);
    }

    public async Task<bool> DeleteAsync(int userId, Guid historyId)
    {
        var entity = await _db.TravelHistories
            .FirstOrDefaultAsync(t => t.Id == historyId && t.UserId == userId);
        
        if (entity == null)
            return false;

        _db.TravelHistories.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TravelHistoryResource MapToResource(Domain.Model.Aggregates.TravelHistory entity)
    {
        var routeObj = !string.IsNullOrWhiteSpace(entity.RouteJson)
            ? JsonSerializer.Deserialize<object>(entity.RouteJson)
            : null;

        return new TravelHistoryResource(
            entity.Id,
            entity.Title,
            entity.OccurredAt,
            entity.DurationMinutes,
            entity.DistanceKm,
            routeObj,
            entity.CreatedAt
        );
    }
}

