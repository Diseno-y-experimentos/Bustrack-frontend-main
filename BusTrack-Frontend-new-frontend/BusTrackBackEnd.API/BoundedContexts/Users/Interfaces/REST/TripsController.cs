using BusTrackBackEnd.API.BoundedContexts.Users.Domain.Model.Aggregates;
using BusTrackBackEnd.API.BoundedContexts.Users.Interfaces.REST.Resources;
using BusTrackBackEnd.API.Shared.Domain.Repositories;
using BusTrackBackEnd.API.Shared.Infrastructure.Persistence.EFC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BusTrackBackEnd.API.BoundedContexts.Users.Interfaces.REST;

[ApiController]
[Authorize]
[Route("api/v1/users/{userId:int}/trips")]
[Route("users/{userId:int}/trips")]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public TripsController(AppDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int userId)
    {
        if (!TryGetTokenUserId(out var tokenUserId)) return Unauthorized();
        if (tokenUserId != userId) return Forbid();

        var trips = await _context.Set<Trip>()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(trips.Select(ToResource));
    }

    [HttpPost]
    public async Task<IActionResult> Create(int userId, [FromBody] CreateTripResource resource)
    {
        if (!TryGetTokenUserId(out var tokenUserId)) return Unauthorized();
        if (tokenUserId != userId) return Forbid();

        var trip = new Trip(userId, resource.RouteId, resource.Origin, resource.Destination, resource.StartedAt, resource.EndedAt, resource.Notes);
        await _context.Set<Trip>().AddAsync(trip);
        await _unitOfWork.CompleteAsync();

        return Ok(ToResource(trip));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int userId, int id)
    {
        if (!TryGetTokenUserId(out var tokenUserId)) return Unauthorized();
        if (tokenUserId != userId) return Forbid();

        var trip = await _context.Set<Trip>().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (trip == null) return NotFound();

        _context.Set<Trip>().Remove(trip);
        await _unitOfWork.CompleteAsync();
        return NoContent();
    }

    private static TripResource ToResource(Trip trip)
        => new(trip.Id, trip.UserId, trip.RouteId, trip.Origin, trip.Destination, trip.StartedAt, trip.EndedAt, trip.Notes, trip.CreatedAt, trip.UpdatedAt);

    private bool TryGetTokenUserId(out int userId)
    {
        userId = 0;
        var claimValue = User.FindFirst("id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claimValue, out userId);
    }
}
