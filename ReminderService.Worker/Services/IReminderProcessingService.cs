namespace ReminderService.Worker.Services;

public interface IReminderProcessingService
{
    Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default);
}
