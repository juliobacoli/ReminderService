namespace ReminderService.Worker.Services;

public interface ITemplateService
{
    string RenderEmailTemplate(string recipientName, string title, string description, DateTime dueDate);
}
