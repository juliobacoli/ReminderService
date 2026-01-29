using ReminderService.Worker.Enums;

namespace ReminderService.Worker.Entities;

public class ReminderRecipient
{
    public int Id { get; set; }
    public int ReminderId { get; set; }
    public int RecipientId { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
    public DateTime? SentAt { get; set; }

    public Reminder Reminder { get; set; } = null!;
    public Recipient Recipient { get; set; } = null!;
}
