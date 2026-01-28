using EnterpriseDataCopilot.Application.Abstractions;
using EnterpriseDataCopilot.Application.Copilot.Time;
using EnterpriseDataCopilot.Application.Copilot.Metrics;
using EnterpriseDataCopilot.Domain.Querying;

namespace EnterpriseDataCopilot.Application.Copilot.Ask;

public sealed class AskCopilotHandler
{
    private readonly ITimeContextResolver _time;
    private readonly IClock _clock;
    private readonly IAuditWriter _audit;
    private readonly ISqlQueryBuilder _sql;

    public AskCopilotHandler(ITimeContextResolver time, IClock clock, IAuditWriter audit, ISqlQueryBuilder sql)
    {
        _time = time;
        _clock = clock;
        _audit = audit;
        _sql = sql;
    }

    public async Task<AskCopilotResponse> HandleAsync(AskCopilotRequest request, CancellationToken ct)
    {
        var question = (request.Question ?? string.Empty).Trim();

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var time = _time.Resolve(question, today);

        const string metricKey = "net_revenue";
        var metric = MetricsRegistry.All[metricKey];

        var plan = new QueryPlan(
            PlanId: Guid.NewGuid().ToString("N"),
            Metric: metric,
            Time: time,
            GroupBy: "Month", 
            Filters: new Dictionary<string, string>()
        );

        var sqlQuery = _sql.Build(plan);

        await _audit.WriteAsync(new AuditEntry(
            TimestampUtc: _clock.UtcNow,
            EventType: "AskPlanned",
            Message: question,
            Meta: new Dictionary<string, string>
            {
                ["planId"] = plan.PlanId,
                ["metricKey"] = metricKey,
                ["timeKind"] = time.Kind,
                ["from"] = time.From.ToString("yyyy-MM-dd"),
                ["to"] = time.To.ToString("yyyy-MM-dd"),
                ["groupBy"] = plan.GroupBy ?? ""
            }
        ), ct);

        return new AskCopilotResponse(plan, sqlQuery);
    }
}
