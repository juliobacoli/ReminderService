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
            new Recipient { Name = "Julio", Email = "juli-microlins@hotmail.com", IsActive = true },
            new Recipient { Name = "Ivan", Email = "ivanpicanha@hotmail.com", IsActive = true },
            new Recipient { Name = "Eliene", Email = "elienerochasantana@gmail.com", IsActive = true },
            new Recipient { Name = "Bianca", Email = "bia.maced@hotmail.com", IsActive = true }
        };

        await context.Recipients.AddRangeAsync(recipients);
        await context.SaveChangesAsync();

        var reminder = new Reminder
        {
            Title = "Renovação Porto Seguro",
            Description = "Lembrete para renovação do seguro Porto Seguro. Verificar condições, coberturas e realizar o pagamento dentro do prazo.",
            DueDate = DateTime.Now.AddDays(65),
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
