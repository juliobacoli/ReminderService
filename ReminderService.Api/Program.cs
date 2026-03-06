using Microsoft.EntityFrameworkCore;
using ReminderService.Api.Endpoints;
using ReminderService.Worker.Data;
using ReminderService.Worker.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Iniciando ReminderService API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        options.UseSqlite(connectionString);
    }, ServiceLifetime.Transient);

    builder.Services.AddScoped<IReminderRepository, ReminderRepository>();

    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.MapReminderEndpoints();
    app.MapRecipientEndpoints();
    app.MapDashboardEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
