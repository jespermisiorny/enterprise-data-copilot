using EnterpriseDataCopilot.Domain.Metrics;

namespace EnterpriseDataCopilot.Application.Copilot.Metrics;

public static class MetricsRegistry
{
    public static readonly IReadOnlyDictionary<string, MetricDefinition> All =
        new Dictionary<string, MetricDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["net_revenue"] = new(
                Key: "net_revenue",
                DisplayName: "Nettoomsättning",
                Description: "SUM(Revenue) enligt business-regel i MVP.",
                SqlExpression: "SUM(f.Revenue)",
                Format: "currency"
            )
        };
}
