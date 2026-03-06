using ReminderService.Worker.Entities;
using ReminderService.Worker.Repositories;

namespace ReminderService.Api.Endpoints;

public static class RecipientEndpoints
{
    public static void MapRecipientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipients");

        group.MapGet("/", async (IReminderRepository repo) =>
        {
            var recipients = await repo.GetAllRecipientsAsync();
            return Results.Ok(recipients.Select(r => new
            {
                r.Id,
                r.Name,
                r.Email,
                r.IsActive
            }));
        });

        group.MapPost("/", async (CreateRecipientRequest request, IReminderRepository repo) =>
        {
            var recipient = new Recipient
            {
                Name = request.Name,
                Email = request.Email
            };

            var created = await repo.CreateRecipientAsync(recipient);
            return Results.Created($"/api/recipients/{created.Id}", new { created.Id, created.Name });
        });

        group.MapPut("/{id}/toggle", async (int id, IReminderRepository repo) =>
        {
            var success = await repo.ToggleRecipientAsync(id);
            return success ? Results.Ok() : Results.NotFound();
        });
    }
}

public record CreateRecipientRequest(string Name, string Email);
