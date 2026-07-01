using BusTrackBackEnd.API.IAM.Interfaces.REST.Resources;

namespace BusTrackBackEnd.API.IAM.Application.Internal.OutboundServices;

public interface ITravelHistoryService
{
    Task<TravelHistoryResource> CreateAsync(int userId, CreateTravelHistoryResource createDto);
    Task<IEnumerable<TravelHistoryResource>> GetForUserAsync(int userId, int page = 1, int pageSize = 20);
    Task<bool> DeleteAsync(int userId, Guid historyId);
}

