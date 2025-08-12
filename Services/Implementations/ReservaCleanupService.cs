using AgenciaDeTransporteWeb.Services.Interfaces;

namespace AgenciaDeTransporteWeb.Services.Implementations
{
    public class ReservaCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservaCleanupService> _logger;

        public ReservaCleanupService(IServiceProvider serviceProvider, ILogger<ReservaCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var reservaService = scope.ServiceProvider.GetRequiredService<IReservaService>();

                    await reservaService.LiberarReservasExpiradas();

                    _logger.LogInformation("Limpieza de reservas expiradas ejecutada a las {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante la limpieza de reservas expiradas");
                }

                // Ejecutar cada 30 minutos
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}