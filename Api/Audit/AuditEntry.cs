namespace Api.Audit;

public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Question { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
}
