namespace EnterpriseDataCopilot.Application.Abstractions;

public interface IAuditWriter
{
    Task WriteAsync(AuditEntry entry, CancellationToken ct);
    Task<IReadOnlyList<AuditEntry>> ReadLatestAsync(int take, CancellationToken ct);
}

public sealed record AuditEntry(
    DateTimeOffset TimestampUtc,
    string EventType,            // "Ask", "Health", etc
    string Message,
    IReadOnlyDictionary<string, string>? Meta = null
);
