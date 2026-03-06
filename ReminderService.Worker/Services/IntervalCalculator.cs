namespace ReminderService.Worker.Services;

public class IntervalCalculator : IIntervalCalculator
{
    public int CalculateInterval(DateTime dueDate, DateTime currentDate, int defaultIntervalDays)
    {
        var daysUntilDue = (dueDate.Date - currentDate.Date).Days;

        if (daysUntilDue <= 0)
            return 0;

        if (daysUntilDue <= 7)
            return 1;

        if (daysUntilDue <= 30)
            return 7;

        if (daysUntilDue <= 90)
            return 15;

        if (daysUntilDue <= 180)
            return 30;

        return defaultIntervalDays;
    }
}
