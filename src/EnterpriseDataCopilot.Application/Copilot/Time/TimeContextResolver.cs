using EnterpriseDataCopilot.Domain.Time;

namespace EnterpriseDataCopilot.Application.Copilot.Time;

public interface ITimeContextResolver
{
    TimeContext Resolve(string input, DateOnly today);
}

public sealed class TimeContextResolver : ITimeContextResolver
{
    public TimeContext Resolve(string input, DateOnly today)
    {
        var q = (input ?? string.Empty).Trim().ToLowerInvariant();

        if (q.Contains("förra året") || q.Contains("föregående år"))
        {
            var year = today.Year - 1;
            return new TimeContext(
                Kind: "PreviousYear",
                From: new DateOnly(year, 1, 1),
                To: new DateOnly(year, 12, 31)
            );
        }

        if (q.Contains("ytd"))
        {
            return new TimeContext(
                Kind: "Ytd",
                From: new DateOnly(today.Year, 1, 1),
                To: today
            );
        }

        // Default: Rolling 12 months
        var from = today.AddMonths(-12).AddDays(1); // inkl idag -> “senaste 12 månader”
        return new TimeContext(
            Kind: "Rolling12Months",
            From: from,
            To: today
        );
    }
}
