using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;
using ReminderService.Worker.Repositories;

namespace ReminderService.Worker.Services;

public class ReminderProcessingService : IReminderProcessingService
{
    private readonly IReminderRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<ReminderProcessingService> _logger;

    public ReminderProcessingService(
        IReminderRepository repository,
        IEmailService emailService,
        ITemplateService templateService,
        ILogger<ReminderProcessingService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        var currentDate = GetCurrentBrazilianTime();
        _logger.LogInformation("Verificando lembretes pendentes em: {CurrentDate}", currentDate);

        var reminders = await _repository.GetPendingRemindersAsync(currentDate);

        if (reminders.Count == 0)
        {
            _logger.LogInformation("Nenhum lembrete pendente encontrado");
            return;
        }

        _logger.LogInformation("Encontrados {Count} lembretes para processar", reminders.Count);

        foreach (var reminder in reminders)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await ProcessSingleReminderAsync(reminder, cancellationToken);
        }
    }

    private async Task ProcessSingleReminderAsync(Reminder reminder, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando lembrete: {Title} (ID: {Id})", reminder.Title, reminder.Id);

        await _repository.ResetReminderRecipientsStatusAsync(reminder.Id);

        var activeRecipients = reminder.ReminderRecipients
            .Where(rr => rr.Recipient.IsActive)
            .ToList();

        _logger.LogInformation("Enviando para {Count} destinatários ativos", activeRecipients.Count);

        var sentCount = 0;

        foreach (var rr in activeRecipients)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var htmlBody = _templateService.RenderEmailTemplate(
                rr.Recipient.Name,
                reminder.Title,
                reminder.Description,
                reminder.DueDate
            );

            var subject = reminder.Priority == ReminderPriority.High
                ? $"[URGENTE] {reminder.Title}"
                : reminder.Title;

            var success = await _emailService.SendEmailAsync(
                rr.Recipient.Email,
                rr.Recipient.Name,
                subject,
                htmlBody
            );

            var status = success ? ReminderStatus.Sent : ReminderStatus.Failed;
            await _repository.UpdateReminderRecipientStatusAsync(rr.Id, status);

            if (success) sentCount++;

            await Task.Delay(1000, cancellationToken);
        }

        if (sentCount > 0)
        {
            var currentDate = GetCurrentBrazilianTime();
            await _repository.UpdateReminderLastSentAtAsync(reminder.Id, currentDate);
            _logger.LogInformation("Lembrete {Id} atualizado. Enviados: {SentCount}/{TotalCount}",
                reminder.Id, sentCount, activeRecipients.Count);
        }
        else
        {
            _logger.LogWarning("Nenhum e-mail enviado com sucesso para o lembrete {Id}", reminder.Id);
        }
    }

    private static DateTime GetCurrentBrazilianTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")
        );
    }
}
