using FluentAssertions;
using ReminderService.Worker.Enums;

namespace ReminderService.Worker.Tests.Enums;

public class ReminderStatusTests
{
    [Fact]
    public void ReminderStatus_DeveConterTodosOsValoresEsperados()
    {
        // Assert
        var values = Enum.GetValues<ReminderStatus>();

        values.Should().HaveCount(3);
        values.Should().Contain(ReminderStatus.Pending);
        values.Should().Contain(ReminderStatus.Sent);
        values.Should().Contain(ReminderStatus.Failed);
    }
}
