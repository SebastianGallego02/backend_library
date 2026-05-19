using System;
using System.Threading;
using System.Threading.Tasks;
using backend_library.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend_library.Infrastructure.Workers;

public class ExpiredLoansWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ExpiredLoansWorker> _logger;

    public ExpiredLoansWorker(IServiceProvider services, ILogger<ExpiredLoansWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de Sanciones Automáticas iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Revisando préstamos vencidos para aplicar sanciones...");

                // Como es un servicio Singleton en segundo plano, creamos un Scope para usar el DbContext
                using (var scope = _services.CreateScope())
                {
                    var loanService = scope.ServiceProvider.GetRequiredService<ILoanService>();
                    await loanService.ProcessExpiredLoansAsync();
                }

                _logger.LogInformation("Revisión de sanciones completada con éxito.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al procesar las sanciones automáticas.");
            }

            // Esperar 24 horas antes de volver a revisar (o el tiempo que prefieras para pruebas, ej. TimeSpan.FromMinutes(5))
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}