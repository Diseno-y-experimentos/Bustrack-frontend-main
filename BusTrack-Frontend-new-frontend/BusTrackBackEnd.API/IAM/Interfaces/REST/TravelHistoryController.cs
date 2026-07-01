using BusTrackBackEnd.API.IAM.Application.Internal.OutboundServices;
using BusTrackBackEnd.API.IAM.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTrackBackEnd.API.IAM.Interfaces.REST;

[ApiController]
[Authorize]
[Route("api/v1/travel-history")]
[Route("travel-history")]
public class TravelHistoryController : ControllerBase
{
    private readonly ITravelHistoryService _travelHistoryService;

    public TravelHistoryController(ITravelHistoryService travelHistoryService)
    {
        _travelHistoryService = travelHistoryService;
    }

    /// <summary>
    /// Crea un nuevo registro en el historial de viajes del usuario autenticado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTravelHistory([FromBody] CreateTravelHistoryResource resource)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var result = await _travelHistoryService.CreateAsync(userId, resource);
            return CreatedAtAction(nameof(GetTravelHistory), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de viajes del usuario autenticado con soporte para paging.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTravelHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var travels = await _travelHistoryService.GetForUserAsync(userId, page, pageSize);
            return Ok(new { data = travels, page, pageSize });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un viaje específico del historial (solo si pertenece al usuario autenticado).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTravelHistoryById(Guid id)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var travels = await _travelHistoryService.GetForUserAsync(userId, 1, int.MaxValue);
            var travel = travels.FirstOrDefault(t => t.Id == id);

            if (travel == null)
                return NotFound(new { message = "Travel not found or not authorized" });

            return Ok(travel);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un registro del historial de viajes (solo si pertenece al usuario autenticado).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTravelHistory(Guid id)
    {
        try
        {
            if (!TryGetCurrentUserId(out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var deleted = await _travelHistoryService.DeleteAsync(userId, id);
            
            if (!deleted)
                return NotFound(new { message = "Travel not found or not authorized" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var claim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        return int.TryParse(claim, out userId);
    }
}

