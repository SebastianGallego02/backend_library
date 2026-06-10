using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_library.Application.Interfaces;
using backend_library.Application.DTOs;

namespace backend_library.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Solo administradores pueden gestionar sanciones
public class SanctionsController : ControllerBase
{
    private readonly ISanctionService _sanctionService;

    public SanctionsController(ISanctionService sanctionService)
    {
        _sanctionService = sanctionService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _sanctionService.GetActiveSanctionsAsync();
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _sanctionService.GetSanctionHistoryAsync();
        return Ok(result);
    }

    [HttpPatch("{id}/remove")]
    public async Task<IActionResult> Remove(int id)
    {
        try
        {
            var result = await _sanctionService.DeactivateSanctionAsync(id);
            return Ok(new { message = "Sanción removida exitosamente", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/extend")]
    public async Task<IActionResult> Extend(int id, [FromBody] ExtendSanctionRequest request)
    {
        try
        {
            var result = await _sanctionService.ExtendSanctionAsync(id, request.Months);
            return Ok(new { message = "Sanción extendida exitosamente", data = result });
        }
        catch (Exception ex) when (ex is KeyNotFoundException or ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}