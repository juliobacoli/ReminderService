using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;

namespace ReminderService.Worker.Repositories;

public interface IReminderRepository
{
    Task<List<Reminder>> GetPendingRemindersAsync(DateTime currentDate);
    Task ResetReminderRecipientsStatusAsync(int reminderId);
    Task UpdateReminderRecipientStatusAsync(int reminderRecipientId, ReminderStatus status);
    Task UpdateReminderLastSentAtAsync(int reminderId, DateTime lastSentAt);

    Task<List<Reminder>> GetAllRemindersAsync();
    Task<Reminder?> GetReminderByIdAsync(int id);
    Task<Reminder> CreateReminderAsync(Reminder reminder, List<int> recipientIds);
    Task<bool> ToggleReminderAsync(int id);

    Task<List<Recipient>> GetAllRecipientsAsync();
    Task<Recipient> CreateRecipientAsync(Recipient recipient);
    Task<bool> ToggleRecipientAsync(int id);
}
