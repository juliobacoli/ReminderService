namespace ReminderService.Worker.Services;

public class TemplateService : ITemplateService
{
    private readonly string _templateCache;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(ILogger<TemplateService> logger)
    {
        _logger = logger;
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "email-template.html");

        if (!File.Exists(templatePath))
        {
            _logger.LogError("Template HTML não encontrado em: {TemplatePath}", templatePath);
            _templateCache = string.Empty;
        }
        else
        {
            _templateCache = File.ReadAllText(templatePath);
            _logger.LogInformation("Template HTML carregado com sucesso");
        }
    }

    public string RenderEmailTemplate(string recipientName, string title, string description, DateTime dueDate)
    {
        if (string.IsNullOrEmpty(_templateCache))
        {
            return $"<h1>{title}</h1><p>{description}</p><p>Vencimento: {dueDate:dd/MM/yyyy}</p>";
        }

        return _templateCache
            .Replace("{{RecipientName}}", recipientName)
            .Replace("{{Title}}", title)
            .Replace("{{Description}}", description)
            .Replace("{{DueDate}}", dueDate.ToString("dd/MM/yyyy"));
    }
}
