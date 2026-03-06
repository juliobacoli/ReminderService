using FluentAssertions;
using ReminderService.Worker.Entities;
using ReminderService.Worker.Enums;
using ReminderService.Worker.Repositories;
using ReminderService.Worker.Tests.Helpers;

namespace ReminderService.Worker.Tests.Repositories;

public class ReminderRepositoryApiTests : IDisposable
{
    private readonly Data.AppDbContext _context;
    private readonly ReminderRepository _repository;

    public ReminderRepositoryApiTests()
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
    public async Task GetAllRemindersAsync_RetornaTodosLembretes()
    {
        _context.Reminders.AddRange(
            new Reminder { Title = "A", Description = "Desc", DueDate = DateTime.Now.AddDays(10), IntervalDays = 30 },
            new Reminder { Title = "B", Description = "Desc", DueDate = DateTime.Now.AddDays(5), IntervalDays = 30 }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllRemindersAsync();

        result.Should().HaveCount(2);
        result[0].DueDate.Should().BeBefore(result[1].DueDate);
    }

    [Fact]
    public async Task GetReminderByIdAsync_RetornaLembreteComRecipients()
    {
        var recipient = new Recipient { Name = "Test", Email = "test@test.com", IsActive = true };
        var reminder = new Reminder { Title = "Test", Description = "Desc", DueDate = DateTime.Now.AddDays(10), IntervalDays = 30 };

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

        var result = await _repository.GetReminderByIdAsync(reminder.Id);

        result.Should().NotBeNull();
        result!.ReminderRecipients.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetReminderByIdAsync_RetornaNullQuandoNaoExiste()
    {
        var result = await _repository.GetReminderByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateReminderAsync_CriaComRecipients()
    {
        var recipient = new Recipient { Name = "Test", Email = "test@test.com", IsActive = true };
        _context.Recipients.Add(recipient);
        await _context.SaveChangesAsync();

        var reminder = new Reminder
        {
            Title = "Novo",
            Description = "Desc",
            DueDate = DateTime.Now.AddDays(30),
            IntervalDays = 15
        };

        var created = await _repository.CreateReminderAsync(reminder, new List<int> { recipient.Id });

        created.Id.Should().BeGreaterThan(0);
        _context.ReminderRecipients.Count(rr => rr.ReminderId == created.Id).Should().Be(1);
    }

    [Fact]
    public async Task ToggleReminderAsync_AlternaAtivo()
    {
        var reminder = new Reminder { Title = "Test", Description = "Desc", DueDate = DateTime.Now.AddDays(10), IntervalDays = 30, IsActive = true };
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        var result = await _repository.ToggleReminderAsync(reminder.Id);

        result.Should().BeTrue();
        var updated = await _context.Reminders.FindAsync(reminder.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleReminderAsync_RetornaFalseQuandoNaoExiste()
    {
        var result = await _repository.ToggleReminderAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllRecipientsAsync_RetornaTodosOrdenados()
    {
        _context.Recipients.AddRange(
            new Recipient { Name = "Zeca", Email = "z@test.com", IsActive = true },
            new Recipient { Name = "Ana", Email = "a@test.com", IsActive = true }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllRecipientsAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Ana");
    }

    [Fact]
    public async Task CreateRecipientAsync_CriaDestinatario()
    {
        var recipient = new Recipient { Name = "Novo", Email = "novo@test.com" };

        var created = await _repository.CreateRecipientAsync(recipient);

        created.Id.Should().BeGreaterThan(0);
        created.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleRecipientAsync_AlternaAtivo()
    {
        var recipient = new Recipient { Name = "Test", Email = "test@test.com", IsActive = true };
        _context.Recipients.Add(recipient);
        await _context.SaveChangesAsync();

        var result = await _repository.ToggleRecipientAsync(recipient.Id);

        result.Should().BeTrue();
        var updated = await _context.Recipients.FindAsync(recipient.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleRecipientAsync_RetornaFalseQuandoNaoExiste()
    {
        var result = await _repository.ToggleRecipientAsync(999);
        result.Should().BeFalse();
    }
}
