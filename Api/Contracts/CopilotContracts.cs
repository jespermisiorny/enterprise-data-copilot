namespace Api.Contracts;

public record CopilotQueryRequest(string Text);

public record CopilotCitation(string Id, string Label, string Snippet);

public record CopilotInsight(
    string Title,
    string Type,
    string Metric,
    double? Delta,
    string Why,
    string[] Evidence
);

public record CopilotResponse(
    string Summary,
    string Confidence,
    CopilotInsight[] Insights,
    CopilotCitation[] Citations,
    string[] FollowUps
);
