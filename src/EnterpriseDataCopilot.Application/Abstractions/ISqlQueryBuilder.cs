using EnterpriseDataCopilot.Domain.Querying;

namespace EnterpriseDataCopilot.Application.Abstractions;

public interface ISqlQueryBuilder
{
    SqlQuery Build(QueryPlan plan);
}
