using FluentAssertions;
using ReminderService.Worker.Services;

namespace ReminderService.Worker.Tests.Services;

public class IntervalCalculatorTests
{
    private readonly IntervalCalculator _calculator = new();
    private const int DefaultInterval = 65;

    [Fact]
    public void CalculateInterval_MaisDe180Dias_RetornaIntervaloPadrao()
    {
        var currentDate = new DateTime(2026, 3, 1);
        var dueDate = new DateTime(2026, 12, 1); // 275 dias

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(DefaultInterval);
    }

    [Fact]
    public void CalculateInterval_Entre90E180Dias_Retorna30()
    {
        var currentDate = new DateTime(2026, 3, 1);
        var dueDate = new DateTime(2026, 7, 1); // 122 dias

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(30);
    }

    [Fact]
    public void CalculateInterval_Entre30E90Dias_Retorna15()
    {
        var currentDate = new DateTime(2026, 3, 1);
        var dueDate = new DateTime(2026, 5, 1); // 61 dias

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(15);
    }

    [Fact]
    public void CalculateInterval_Entre7E30Dias_Retorna7()
    {
        var currentDate = new DateTime(2026, 3, 1);
        var dueDate = new DateTime(2026, 3, 20); // 19 dias

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(7);
    }

    [Fact]
    public void CalculateInterval_MenosDe7Dias_Retorna1()
    {
        var currentDate = new DateTime(2026, 3, 1);
        var dueDate = new DateTime(2026, 3, 5); // 4 dias

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(1);
    }

    [Fact]
    public void CalculateInterval_DueDateNoPassado_Retorna0()
    {
        var currentDate = new DateTime(2026, 3, 10);
        var dueDate = new DateTime(2026, 3, 5); // -5 dias

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(0);
    }

    // Boundary cases

    [Fact]
    public void CalculateInterval_Exatamente180Dias_Retorna30()
    {
        var currentDate = new DateTime(2026, 1, 1);
        var dueDate = new DateTime(2026, 7, 1); // 181 dias

        // 180 dias exatos
        var dueDate180 = currentDate.AddDays(180);
        var result = _calculator.CalculateInterval(dueDate180, currentDate, DefaultInterval);

        result.Should().Be(30);
    }

    [Fact]
    public void CalculateInterval_Exatamente90Dias_Retorna15()
    {
        var currentDate = new DateTime(2026, 1, 1);
        var dueDate = currentDate.AddDays(90);

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(15);
    }

    [Fact]
    public void CalculateInterval_Exatamente30Dias_Retorna7()
    {
        var currentDate = new DateTime(2026, 1, 1);
        var dueDate = currentDate.AddDays(30);

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(7);
    }

    [Fact]
    public void CalculateInterval_Exatamente7Dias_Retorna1()
    {
        var currentDate = new DateTime(2026, 1, 1);
        var dueDate = currentDate.AddDays(7);

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(1);
    }

    [Fact]
    public void CalculateInterval_MesmoDia_Retorna0()
    {
        var currentDate = new DateTime(2026, 3, 1);
        var dueDate = new DateTime(2026, 3, 1);

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateInterval_Exatamente181Dias_RetornaIntervaloPadrao()
    {
        var currentDate = new DateTime(2026, 1, 1);
        var dueDate = currentDate.AddDays(181);

        var result = _calculator.CalculateInterval(dueDate, currentDate, DefaultInterval);

        result.Should().Be(DefaultInterval);
    }
}
