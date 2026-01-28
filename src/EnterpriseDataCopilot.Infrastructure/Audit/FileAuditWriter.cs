using System.Text.Json;
using EnterpriseDataCopilot.Application.Abstractions;

namespace EnterpriseDataCopilot.Infrastructure.Audit;

public sealed class FileAuditWriter : IAuditWriter
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public FileAuditWriter(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken ct)
    {
        var line = JsonSerializer.Serialize(entry, JsonOptions);
        await File.AppendAllTextAsync(_path, line + Environment.NewLine, ct);
    }

    public async Task<IReadOnlyList<AuditEntry>> ReadLatestAsync(int take, CancellationToken ct)
    {
        if (!File.Exists(_path)) return Array.Empty<AuditEntry>();

        // Enkel MVP: läs alla, ta sista N
        var lines = await File.ReadAllLinesAsync(_path, ct);
        var slice = lines.Reverse().Take(take).Reverse();

        var list = new List<AuditEntry>();
        foreach (var line in slice)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<AuditEntry>(line, JsonOptions);
                if (entry is not null) list.Add(entry);
            }
            catch { /* ignore MVP */ }
        }

        return list;
    }
}
