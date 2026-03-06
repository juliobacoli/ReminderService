using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;
using ReminderService.Worker.Services;

namespace ReminderService.Worker.Repositories;

public interface IReminderRepository
{
    Task<List<Reminder>> GetPendingRemindersAsync(DateTime currentDate, IIntervalCalculator intervalCalculator);
    Task ResetReminderRecipientsStatusAsync(int reminderId);
    Task UpdateReminderRecipientStatusAsync(int reminderRecipientId, ReminderStatus status);
    Task UpdateReminderLastSentAtAsync(int reminderId, DateTime lastSentAt);
}
