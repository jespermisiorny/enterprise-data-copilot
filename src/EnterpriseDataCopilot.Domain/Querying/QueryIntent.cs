namespace EnterpriseDataCopilot.Domain.Querying;

public sealed record QueryIntent(
    string MetricKey,
    string? GroupBy = null,
    IReadOnlyDictionary<string, string>? Filters = null
);
