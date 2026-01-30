using Microsoft.EntityFrameworkCore;
using ReminderService.Worker.Data;
using ReminderService.Worker.Enums;

namespace ReminderService.Worker;

public static class TestHelper
{
    public static async Task PrepareForImmediateTestAsync(AppDbContext context)
    {
        // Atualizar lembrete para vencer AGORA
        var reminder = await context.Reminders.FirstOrDefaultAsync(r => r.Id == 1);
        if (reminder != null)
        {
            reminder.DueDate = DateTime.Now;
            reminder.LastSentAt = null;
            Console.WriteLine($"✅ Lembrete '{reminder.Title}' atualizado para vencer AGORA");
        }

        // Resetar status dos destinatários
        var reminderRecipients = await context.ReminderRecipients
            .Where(rr => rr.ReminderId == 1)
            .ToListAsync();

        foreach (var rr in reminderRecipients)
        {
            rr.Status = ReminderStatus.Pending;
            rr.SentAt = null;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {reminderRecipients.Count} destinatários resetados para Pending");
        Console.WriteLine("🚀 Pronto para enviar e-mails!");
    }
}
