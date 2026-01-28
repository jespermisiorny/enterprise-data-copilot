using EnterpriseDataCopilot.Domain.Querying;

namespace EnterpriseDataCopilot.Application.Copilot.Ask;

public sealed record AskCopilotResponse(
    QueryPlan Plan,
    SqlQuery Sql
);
