namespace EnterpriseDataCopilot.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
