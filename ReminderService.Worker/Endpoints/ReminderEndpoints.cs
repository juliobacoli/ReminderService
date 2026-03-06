using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;
using ReminderService.Worker.Repositories;

namespace ReminderService.Worker.Endpoints;

public static class ReminderEndpoints
{
    public static void MapReminderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reminders");

        group.MapGet("/", async (IReminderRepository repo) =>
        {
            var reminders = await repo.GetAllRemindersAsync();
            return Results.Ok(reminders.Select(r => new
            {
                r.Id,
                r.Title,
                r.Description,
                DueDate = r.DueDate.ToString("yyyy-MM-dd"),
                r.IntervalDays,
                LastSentAt = r.LastSentAt?.ToString("yyyy-MM-dd HH:mm"),
                Priority = r.Priority.ToString(),
                r.IsActive,
                Recipients = r.ReminderRecipients.Select(rr => new
                {
                    rr.Recipient.Id,
                    rr.Recipient.Name,
                    rr.Recipient.Email,
                    Status = rr.Status.ToString(),
                    SentAt = rr.SentAt?.ToString("yyyy-MM-dd HH:mm")
                })
            }));
        });

        group.MapPost("/", async (CreateReminderRequest request, IReminderRepository repo) =>
        {
            var reminder = new Reminder
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                IntervalDays = request.IntervalDays,
                Priority = request.Priority
            };

            var created = await repo.CreateReminderAsync(reminder, request.RecipientIds);
            return Results.Created($"/api/reminders/{created.Id}", new { created.Id, created.Title });
        });

        group.MapPut("/{id}/toggle", async (int id, IReminderRepository repo) =>
        {
            var success = await repo.ToggleReminderAsync(id);
            return success ? Results.Ok() : Results.NotFound();
        });
    }
}

public record CreateReminderRequest(
    string Title,
    string Description,
    DateTime DueDate,
    int IntervalDays,
    ReminderPriority Priority,
    List<int> RecipientIds
);
