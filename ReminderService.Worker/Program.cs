using Microsoft.EntityFrameworkCore;
using ReminderService.Worker;
using ReminderService.Worker.Data;
using ReminderService.Worker.Repositories;
using ReminderService.Worker.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Iniciando ReminderService Worker");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        options.UseSqlite(connectionString);
    }, ServiceLifetime.Transient);

    builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ITemplateService, TemplateService>();
    builder.Services.AddScoped<IReminderProcessingService, ReminderProcessingService>();

    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();

    // Modo de teste: preparar banco para envio imediato
    if (args.Contains("--prepare-test"))
    {
        Log.Information(" Modo de teste ativado - preparando banco de dados...");
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestHelper.PrepareForImmediateTestAsync(context);
        Log.Information(" Banco preparado! Execute sem --prepare-test para enviar e-mails");
        return;
    }

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
