using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;

namespace ReminderService.Worker.Repositories;

public interface IReminderRepository
{
    Task<List<Reminder>> GetPendingRemindersAsync(DateTime currentDate);
    Task ResetReminderRecipientsStatusAsync(int reminderId);
    Task UpdateReminderRecipientStatusAsync(int reminderRecipientId, ReminderStatus status);
    Task UpdateReminderLastSentAtAsync(int reminderId, DateTime lastSentAt);
}
