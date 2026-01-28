namespace EnterpriseDataCopilot.Domain.Metrics;

public sealed record MetricDefinition(
    string Key,
    string DisplayName,
    string Description,
    string SqlExpression,
    string Format
);
