namespace ReminderService.Worker.Entities;

public class Recipient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ReminderRecipient> ReminderRecipients { get; set; } = new List<ReminderRecipient>();
}
