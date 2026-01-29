namespace ReminderService.Worker.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlBody);
}
