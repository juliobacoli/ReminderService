using ReminderService.Worker.Enums;

namespace ReminderService.Worker.Entities;

public class Reminder
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int IntervalDays { get; set; }
    public DateTime? LastSentAt { get; set; }
    public ReminderPriority Priority { get; set; } = ReminderPriority.Normal;

    public ICollection<ReminderRecipient> ReminderRecipients { get; set; } = new List<ReminderRecipient>();
}
