namespace EnterpriseDataCopilot.Domain.Querying;

public sealed record SqlQuery(
    string Text,
    IReadOnlyDictionary<string, object> Parameters
);
