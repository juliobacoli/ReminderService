using ReminderService.Worker.Enums;
using ReminderService.Worker.Repositories;

namespace ReminderService.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/dashboard", async (IReminderRepository repo) =>
        {
            var reminders = await repo.GetAllRemindersAsync();
            var recipients = await repo.GetAllRecipientsAsync();

            return Results.Ok(new
            {
                TotalReminders = reminders.Count,
                ActiveReminders = reminders.Count(r => r.IsActive && r.DueDate.Date >= DateTime.Now.Date),
                ExpiredReminders = reminders.Count(r => r.DueDate.Date < DateTime.Now.Date),
                TotalRecipients = recipients.Count,
                ActiveRecipients = recipients.Count(r => r.IsActive),
                Reminders = reminders.Select(r => new
                {
                    r.Id,
                    r.Title,
                    DueDate = r.DueDate.ToString("yyyy-MM-dd"),
                    DaysUntilDue = (r.DueDate.Date - DateTime.Now.Date).Days,
                    r.IsActive,
                    LastSentAt = r.LastSentAt?.ToString("yyyy-MM-dd HH:mm"),
                    TotalSent = r.ReminderRecipients.Count(rr => rr.Status == ReminderStatus.Sent),
                    TotalFailed = r.ReminderRecipients.Count(rr => rr.Status == ReminderStatus.Failed),
                    TotalPending = r.ReminderRecipients.Count(rr => rr.Status == ReminderStatus.Pending)
                })
            });
        });
    }
}
