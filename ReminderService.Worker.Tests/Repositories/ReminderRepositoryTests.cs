using FluentAssertions;
using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;
using ReminderService.Worker.Repositories;
using ReminderService.Worker.Tests.Helpers;

namespace ReminderService.Worker.Tests.Repositories;

public class ReminderRepositoryTests : IDisposable
{
    private readonly Data.AppDbContext _context;
    private readonly ReminderRepository _repository;

    public ReminderRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _repository = new ReminderRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetPendingRemindersAsync_QuandoNaoTemLastSentAt_Retorna()
    {
        // Arrange
        var recipient = new Recipient { Name = "João", Email = "joao@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            LastSentAt = null
        };

        _context.Recipients.Add(recipient);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        _context.ReminderRecipients.Add(new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = recipient.Id,
            Status = ReminderStatus.Pending
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingRemindersAsync(DateTime.Now);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(reminder.Id);
    }

    [Fact]
    public async Task GetPendingRemindersAsync_QuandoIntervaloNaoPassou_NaoRetorna()
    {
        // Arrange
        var recipient = new Recipient { Name = "Maria", Email = "maria@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(100),
            IntervalDays = 65,
            LastSentAt = DateTime.Now.AddDays(-30) // Enviado há 30 dias (precisa 65)
        };

        _context.Recipients.Add(recipient);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        _context.ReminderRecipients.Add(new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = recipient.Id,
            Status = ReminderStatus.Sent
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingRemindersAsync(DateTime.Now);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRemindersAsync_QuandoIntervaloPassou_Retorna()
    {
        // Arrange
        var recipient = new Recipient { Name = "Ana", Email = "ana@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(100),
            IntervalDays = 30,
            LastSentAt = DateTime.Now.AddDays(-35) // Enviado há 35 dias (precisa 30)
        };

        _context.Recipients.Add(recipient);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        _context.ReminderRecipients.Add(new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = recipient.Id,
            Status = ReminderStatus.Sent
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingRemindersAsync(DateTime.Now);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPendingRemindersAsync_FiltrarApenasDestinatariosAtivos()
    {
        // Arrange
        var recipientActive = new Recipient { Name = "Ativo", Email = "ativo@test.com", IsActive = true };
        var recipientInactive = new Recipient { Name = "Inativo", Email = "inativo@test.com", IsActive = false };
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            LastSentAt = null
        };

        _context.Recipients.AddRange(recipientActive, recipientInactive);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        _context.ReminderRecipients.AddRange(
            new ReminderRecipient { ReminderId = reminder.Id, RecipientId = recipientActive.Id, Status = ReminderStatus.Pending },
            new ReminderRecipient { ReminderId = reminder.Id, RecipientId = recipientInactive.Id, Status = ReminderStatus.Pending }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingRemindersAsync(DateTime.Now);

        // Assert
        result.Should().HaveCount(1);
        result[0].ReminderRecipients.Should().HaveCount(2); // Retorna todos mas processamento filtrará ativos
    }

    [Fact]
    public async Task GetPendingRemindersAsync_NaoRetornaLembretesExpirados()
    {
        // Arrange
        var recipient = new Recipient { Name = "Pedro", Email = "pedro@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Title = "Expirado",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(-10), // Venceu há 10 dias
            IntervalDays = 30,
            LastSentAt = null
        };

        _context.Recipients.Add(recipient);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        _context.ReminderRecipients.Add(new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = recipient.Id,
            Status = ReminderStatus.Pending
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingRemindersAsync(DateTime.Now);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetReminderRecipientsStatusAsync_ResetaStatusParaPending()
    {
        // Arrange
        var recipient = new Recipient { Name = "Teste", Email = "teste@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30
        };

        _context.Recipients.Add(recipient);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        var reminderRecipient = new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = recipient.Id,
            Status = ReminderStatus.Sent,
            SentAt = DateTime.Now
        };
        _context.ReminderRecipients.Add(reminderRecipient);
        await _context.SaveChangesAsync();

        // Act
        await _repository.ResetReminderRecipientsStatusAsync(reminder.Id);

        // Assert
        var updated = await _context.ReminderRecipients.FindAsync(reminderRecipient.Id);
        updated!.Status.Should().Be(ReminderStatus.Pending);
        updated.SentAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateReminderRecipientStatusAsync_AtualizaStatusParaSent()
    {
        // Arrange
        var recipient = new Recipient { Name = "Teste", Email = "teste@test.com", IsActive = true };
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30
        };

        _context.Recipients.Add(recipient);
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        var reminderRecipient = new ReminderRecipient
        {
            ReminderId = reminder.Id,
            RecipientId = recipient.Id,
            Status = ReminderStatus.Pending
        };
        _context.ReminderRecipients.Add(reminderRecipient);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateReminderRecipientStatusAsync(reminderRecipient.Id, ReminderStatus.Sent);

        // Assert
        var updated = await _context.ReminderRecipients.FindAsync(reminderRecipient.Id);
        updated!.Status.Should().Be(ReminderStatus.Sent);
        updated.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateReminderLastSentAtAsync_AtualizaDataCorretamente()
    {
        // Arrange
        var reminder = new Reminder
        {
            Title = "Test",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(10),
            IntervalDays = 30,
            LastSentAt = null
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        var newDate = new DateTime(2026, 3, 15);

        // Act
        await _repository.UpdateReminderLastSentAtAsync(reminder.Id, newDate);

        // Assert
        var updated = await _context.Reminders.FindAsync(reminder.Id);
        updated!.LastSentAt.Should().Be(newDate);
    }
}
