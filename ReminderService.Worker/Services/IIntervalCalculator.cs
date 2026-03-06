namespace ReminderService.Worker.Services;

public interface IIntervalCalculator
{
    int CalculateInterval(DateTime dueDate, DateTime currentDate, int defaultIntervalDays);
}
