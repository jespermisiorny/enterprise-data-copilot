namespace EnterpriseDataCopilot.Domain.Time;

public sealed record TimeContext(
    string Kind,     // "PreviousYear", "Rolling12Months", "Ytd"
    DateOnly From,
    DateOnly To
);
