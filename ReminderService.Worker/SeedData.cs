using ReminderService.Worker.Data;
using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;

namespace ReminderService.Worker;

public static class SeedData
{
    public static async Task LoadSeedDataAsync(AppDbContext context)
    {
        if (context.Recipients.Any())
        {
            return;
        }

        var recipients = new List<Recipient>
        {
            new Recipient { Name = "Julio", Email = "julio-microlins@hotmail.com", IsActive = true },
            new Recipient { Name = "Ivan", Email = "ivanpicanha@hotmail.com", IsActive = true },
            new Recipient { Name = "Eliene", Email = "elienerochasantana@gmail.com", IsActive = true },
            new Recipient { Name = "Bianca", Email = "bianca.macedo70@outlook.com", IsActive = true }
        };

        await context.Recipients.AddRangeAsync(recipients);
        await context.SaveChangesAsync();

        var reminder = new Reminder
        {
            Title = "FÉRIAS Porto Seguro",
            Description = "Lembrete para as FÉRIAS Porto Seguro.<br><br>" +
                  "• Verificar passagens<br>" +
                  "• Pesquisar passeios<br>" +
                  "• Fazer reserva de carro com antecedência<br><br>" +
                  "Este lembrete será enviado automaticamente a cada 65 dias.",
            DueDate = new DateTime(2026, 12, 1),
            IntervalDays = 65,
            LastSentAt = null,
            Priority = ReminderPriority.Normal
        };

        await context.Reminders.AddAsync(reminder);
        await context.SaveChangesAsync();

        var reminderRecipients = recipients.Select(r => new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = r.Id,
            Status = ReminderStatus.Pending,
            SentAt = null
        }).ToList();

        await context.ReminderRecipients.AddRangeAsync(reminderRecipients);
        await context.SaveChangesAsync();
    }
}
