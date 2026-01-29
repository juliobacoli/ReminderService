using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;
using ReminderService.Worker.Repositories;
using ReminderService.Worker.Services;

namespace ReminderService.Worker.Tests.Services;

public class ReminderProcessingServiceTests
{
    private readonly Mock<IReminderRepository> _repositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ITemplateService> _templateServiceMock;
    private readonly Mock<ILogger<ReminderProcessingService>> _loggerMock;
    private readonly ReminderProcessingService _service;

    public ReminderProcessingServiceTests()
    {
        _repositoryMock = new Mock<IReminderRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _templateServiceMock = new Mock<ITemplateService>();
        _loggerMock = new Mock<ILogger<ReminderProcessingService>>();

        _service = new ReminderProcessingService(
            _repositoryMock.Object,
            _emailServiceMock.Object,
            _templateServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_QuandoNaoTemLembretes_NaoProcessaNada()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder>());

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_AtualizaLastSentAt_ApenasSeEnviarComSucesso()
    {
        // Arrange
        var recipient = new Recipient { Id = 1, Name = "João", Email = "joao@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Id = 1,
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            Priority = ReminderPriority.Normal,
            ReminderRecipients = new List<ReminderRecipient>
            {
                new ReminderRecipient { Id = 1, ReminderId = 1, RecipientId = 1, Recipient = recipient }
            }
        };

        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _templateServiceMock
            .Setup(t => t.RenderEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("<html>Test</html>");

        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _repositoryMock.Verify(r => r.UpdateReminderLastSentAtAsync(reminder.Id, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_NaoAtualizaLastSentAt_QuandoTodosEnviosFalham()
    {
        // Arrange
        var recipient = new Recipient { Id = 1, Name = "João", Email = "joao@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Id = 1,
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            Priority = ReminderPriority.Normal,
            ReminderRecipients = new List<ReminderRecipient>
            {
                new ReminderRecipient { Id = 1, ReminderId = 1, RecipientId = 1, Recipient = recipient }
            }
        };

        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _templateServiceMock
            .Setup(t => t.RenderEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("<html>Test</html>");

        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false); // Falha no envio

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _repositoryMock.Verify(r => r.UpdateReminderLastSentAtAsync(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_ProcessaApenasDestinatariosAtivos()
    {
        // Arrange
        var recipientActive = new Recipient { Id = 1, Name = "Ativo", Email = "ativo@test.com", IsActive = true };
        var recipientInactive = new Recipient { Id = 2, Name = "Inativo", Email = "inativo@test.com", IsActive = false };

        var reminder = new Reminder
        {
            Id = 1,
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            Priority = ReminderPriority.Normal,
            ReminderRecipients = new List<ReminderRecipient>
            {
                new ReminderRecipient { Id = 1, ReminderId = 1, RecipientId = 1, Recipient = recipientActive },
                new ReminderRecipient { Id = 2, ReminderId = 1, RecipientId = 2, Recipient = recipientInactive }
            }
        };

        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _templateServiceMock
            .Setup(t => t.RenderEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("<html>Test</html>");

        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once); // Apenas 1 destinatário ativo
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_AdicionaPrefixoUrgente_QuandoPrioridadeHigh()
    {
        // Arrange
        var recipient = new Recipient { Id = 1, Name = "João", Email = "joao@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Id = 1,
            Title = "Pagamento Urgente",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(1),
            IntervalDays = 30,
            Priority = ReminderPriority.High,
            ReminderRecipients = new List<ReminderRecipient>
            {
                new ReminderRecipient { Id = 1, ReminderId = 1, RecipientId = 1, Recipient = recipient }
            }
        };

        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _templateServiceMock
            .Setup(t => t.RenderEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("<html>Test</html>");

        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _emailServiceMock.Verify(e => e.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            "[URGENTE] Pagamento Urgente",
            It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_ResetaStatusAntesDeProcesar()
    {
        // Arrange
        var recipient = new Recipient { Id = 1, Name = "João", Email = "joao@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Id = 1,
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            Priority = ReminderPriority.Normal,
            ReminderRecipients = new List<ReminderRecipient>
            {
                new ReminderRecipient { Id = 1, ReminderId = 1, RecipientId = 1, Recipient = recipient }
            }
        };

        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _templateServiceMock
            .Setup(t => t.RenderEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("<html>Test</html>");

        _emailServiceMock
            .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _repositoryMock.Verify(r => r.ResetReminderRecipientsStatusAsync(reminder.Id), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingRemindersAsync_AtualizaStatusIndividual_ParaCadaDestinatario()
    {
        // Arrange
        var recipient1 = new Recipient { Id = 1, Name = "João", Email = "joao@test.com", IsActive = true };
        var recipient2 = new Recipient { Id = 2, Name = "Maria", Email = "maria@test.com", IsActive = true };

        var reminder = new Reminder
        {
            Id = 1,
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            Priority = ReminderPriority.Normal,
            ReminderRecipients = new List<ReminderRecipient>
            {
                new ReminderRecipient { Id = 1, ReminderId = 1, RecipientId = 1, Recipient = recipient1 },
                new ReminderRecipient { Id = 2, ReminderId = 1, RecipientId = 2, Recipient = recipient2 }
            }
        };

        _repositoryMock
            .Setup(r => r.GetPendingRemindersAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _templateServiceMock
            .Setup(t => t.RenderEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("<html>Test</html>");

        _emailServiceMock
            .SetupSequence(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true)  // Primeiro sucesso
            .ReturnsAsync(false); // Segundo falha

        // Act
        await _service.ProcessPendingRemindersAsync();

        // Assert
        _repositoryMock.Verify(r => r.UpdateReminderRecipientStatusAsync(1, ReminderStatus.Sent), Times.Once);
        _repositoryMock.Verify(r => r.UpdateReminderRecipientStatusAsync(2, ReminderStatus.Failed), Times.Once);
    }
}
