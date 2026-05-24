using System;
using System.Threading.Tasks;
using backend_library.Application.DTOs;
using backend_library.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_library.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly LoanService _loanService;

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