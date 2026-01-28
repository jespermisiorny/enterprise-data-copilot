namespace Api.Audit;

public class InMemoryAuditStore
{
    private readonly List<AuditEntry> _entries = new();

    public void Add(AuditEntry entry)
    {
        _entries.Insert(0, entry); // senaste först
    }

    public IReadOnlyList<AuditEntry> GetAll()
    {
        return _entries;
    }
}
