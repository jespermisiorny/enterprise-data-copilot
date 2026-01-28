using EnterpriseDataCopilot.Application.Abstractions;
using EnterpriseDataCopilot.Domain.Querying;

namespace EnterpriseDataCopilot.Infrastructure.Data.Sql;

public sealed class SqlQueryBuilder : ISqlQueryBuilder
{
    public SqlQuery Build(QueryPlan plan)
    {
        var p = new Dictionary<string, object>
        {
            ["from"] = plan.Time.From.ToDateTime(TimeOnly.MinValue),
            ["to"] = plan.Time.To.ToDateTime(TimeOnly.MaxValue)
        };

        var groupBy = (plan.GroupBy ?? "").Trim();
        if (!string.IsNullOrEmpty(groupBy) &&
            !groupBy.Equals("Month", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported groupBy: {groupBy}");
        }

        var hasMonthGrouping = groupBy.Equals("Month", StringComparison.OrdinalIgnoreCase);

        var selectParts = new List<string>();
        var groupByParts = new List<string>();
        var orderByParts = new List<string>();

        if (hasMonthGrouping)
        {
            selectParts.Add("t.Year");
            selectParts.Add("t.Month");
            groupByParts.Add("t.Year");
            groupByParts.Add("t.Month");
            orderByParts.Add("t.Year");
            orderByParts.Add("t.Month");
        }

        // Metric expression kommer från registry (t.ex. SUM(f.Revenue))
        selectParts.Add($"{plan.Metric.SqlExpression} AS MetricValue");

        // Bas-SQL
        var sql = $@"
                SELECT
                  {string.Join(",\n  ", selectParts)}
                FROM FactSales f
                JOIN DimTime t ON f.TimeKey = t.TimeKey
                WHERE t.[Date] >= @from AND t.[Date] <= @to
                ".Trim();

        // Filters (MVP: bara “=” på dimfält)
        // plan.Filters kan senare bli typed filters; här är det medvetet simpelt.
        if (plan.Filters is { Count: > 0 })
        {
            foreach (var kv in plan.Filters)
            {
                var key = kv.Key.Trim();
                var value = kv.Value;

                // Parametrar: @filter_<key>
                var paramName = $"filter_{SanitizeKey(key)}";
                p[paramName] = value;

                // OBS: detta är “MVP safe-ish” men inte perfekt.
                // Nästa steg blir att validera filters mot whitelist.
                sql += $"\nAND {key} = @{paramName}";
            }
        }

        if (groupByParts.Count > 0)
        {
            sql += $"\nGROUP BY {string.Join(", ", groupByParts)}";
        }

        if (orderByParts.Count > 0)
        {
            sql += $"\nORDER BY {string.Join(", ", orderByParts)}";
        }

        sql += ";";

        return new SqlQuery(sql, p);
    }

    private static string SanitizeKey(string key)
    {
        // Minimalt för att få stabila parameternamn
        // (tar bort whitespace och “konstiga” tecken)
        var chars = key.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
        return new string(chars);
    }
}
