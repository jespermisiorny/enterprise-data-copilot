using EnterpriseDataCopilot.Domain.Metrics;
using EnterpriseDataCopilot.Domain.Time;

namespace EnterpriseDataCopilot.Domain.Querying;

public sealed record QueryPlan(
    string PlanId,
    MetricDefinition Metric,
    TimeContext Time,
    string? GroupBy,
    IReadOnlyDictionary<string, string> Filters
);
