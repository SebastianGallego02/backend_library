using System;
using System.Security.Claims;
using System.Threading.Tasks;
using backend_library.Application.DTOs;
using backend_library.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Mvc;

namespace backend_library.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔒 Requiere autenticación para cualquier acción de préstamos
public class LoansController : ControllerBase
{
    // ✅ Como debe quedar (con la "I")
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoan([FromBody] CreateLoanRequestDto request)
    {
        try
        {
            var result = await _loanService.CreateLoanAsync(request);
            return CreatedAtAction(nameof(CreateLoan), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            // Captura los errores de reglas de negocio (libro agotado o estudiante sancionado)
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "P0001")
        {
            // P0001 es el código de PostgreSQL para 'raise_exception'
            return BadRequest(new { message = pgEx.MessageText });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ocurrió un error interno", error = ex.Message });
        }
    }

    [HttpPut("{id}/renew")]
    public async Task<IActionResult> RenewLoan(int id)
    {
        try
        {
            var result = await _loanService.RenewLoanAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al renovar el préstamo", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLoans([FromQuery] Guid? userId = null)
    {
        try
        {
            // 🛡️ Regla de seguridad: Si no es Admin, forzamos el filtrado por su propio ID
            if (!User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario del token." });

                userId = Guid.Parse(currentUserId);
            }

            var loans = await _loanService.GetLoansAsync(userId);
            return Ok(loans);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener los préstamos", error = ex.Message });
        }
    }

    [HttpPost("return")]
    public async Task<IActionResult> ReturnLoan([FromBody] ReturnLoanRequestDto request)
    {
        try
        {
            var result = await _loanService.ReturnLoanAsync(request);
            return Ok(new { message = "Libro devuelto con éxito y puesto en disponibilidad.", data = result });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno al procesar la devolución.", error = ex.Message });
        }
    }
}