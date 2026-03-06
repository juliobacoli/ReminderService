using Microsoft.EntityFrameworkCore;
using ReminderService.Worker.Data;
using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;

namespace ReminderService.Worker.Repositories;

public class ReminderRepository : IReminderRepository
{
    private readonly AppDbContext _context;

    public ReminderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reminder>> GetPendingRemindersAsync(DateTime currentDate)
    {
        var allReminders = await _context.Reminders
            .AsNoTracking()
            .Include(r => r.ReminderRecipients)
            .ThenInclude(rr => rr.Recipient)
            .Where(r => r.DueDate.Date >= currentDate.Date)
            .ToListAsync();

        return allReminders
            .Where(r =>
                r.ReminderRecipients.Any(rr => rr.Recipient.IsActive) &&
                (r.LastSentAt == null || (currentDate - r.LastSentAt.Value).TotalDays >= r.IntervalDays)
            ).ToList();
    }

    public async Task ResetReminderRecipientsStatusAsync(int reminderId)
    {
        var reminderRecipients = await _context.ReminderRecipients
            .AsTracking()
            .Where(rr => rr.ReminderId == reminderId)
            .ToListAsync();

        foreach (var rr in reminderRecipients)
        {
            rr.Status = ReminderStatus.Pending;
            rr.SentAt = null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateReminderRecipientStatusAsync(int reminderRecipientId, ReminderStatus status)
    {
        var reminderRecipient = await _context.ReminderRecipients
            .AsTracking()
            .FirstOrDefaultAsync(rr => rr.Id == reminderRecipientId);

        if (reminderRecipient != null)
        {
            reminderRecipient.Status = status;
            reminderRecipient.SentAt = status == ReminderStatus.Sent ? DateTime.Now : null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateReminderLastSentAtAsync(int reminderId, DateTime lastSentAt)
    {
        var reminder = await _context.Reminders
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == reminderId);

        if (reminder != null)
        {
            reminder.LastSentAt = lastSentAt;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Reminder>> GetAllRemindersAsync()
    {
        return await _context.Reminders
            .AsNoTracking()
            .Include(r => r.ReminderRecipients)
            .ThenInclude(rr => rr.Recipient)
            .OrderBy(r => r.DueDate)
            .ToListAsync();
    }

    public async Task<Reminder?> GetReminderByIdAsync(int id)
    {
        return await _context.Reminders
            .AsNoTracking()
            .Include(r => r.ReminderRecipients)
            .ThenInclude(rr => rr.Recipient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reminder> CreateReminderAsync(Reminder reminder, List<int> recipientIds)
    {
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        var reminderRecipients = recipientIds.Select(rid => new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = rid,
            Status = ReminderStatus.Pending
        }).ToList();

        _context.ReminderRecipients.AddRange(reminderRecipients);
        await _context.SaveChangesAsync();

        return reminder;
    }

    public async Task<bool> ToggleReminderAsync(int id)
    {
        var reminder = await _context.Reminders
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reminder == null) return false;

        reminder.IsActive = !reminder.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Recipient>> GetAllRecipientsAsync()
    {
        return await _context.Recipients
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Recipient> CreateRecipientAsync(Recipient recipient)
    {
        _context.Recipients.Add(recipient);
        await _context.SaveChangesAsync();
        return recipient;
    }

    public async Task<bool> ToggleRecipientAsync(int id)
    {
        var recipient = await _context.Recipients
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipient == null) return false;

        recipient.IsActive = !recipient.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
