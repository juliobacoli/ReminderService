using Microsoft.EntityFrameworkCore;
using ReminderService.Worker.Data;
using ReminderService.Worker.Services;

namespace ReminderService.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service iniciado em: {Time}", DateTime.Now);

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync(stoppingToken);
            _logger.LogInformation("Banco de dados verificado/criado com sucesso");

            await SeedData.LoadSeedDataAsync(context);
            _logger.LogInformation("Dados de seed carregados (se necessário)");
        }

        var intervalMinutes = int.Parse(_configuration["WorkerSettings:IntervalInMinutes"] ?? "360");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<IReminderProcessingService>();
                await processingService.ProcessPendingRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar lembretes");
            }

            _logger.LogInformation("Próxima verificação em {Interval} minutos", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}
