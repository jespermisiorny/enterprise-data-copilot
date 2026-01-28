using System.Text;

namespace Api.Docs;

public class DocumentLoader
{
    private readonly List<DocEntry> _documents = new();

    public DocumentLoader()
    {
        LoadDocuments();
    }

    public IReadOnlyList<DocEntry> GetAll()
    {
        return _documents;
    }

    private void LoadDocuments()
    {
        var docsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "docs"
        );

        if (!Directory.Exists(docsPath))
            return;

        foreach (var file in Directory.GetFiles(docsPath, "*.md"))
        {
            var content = File.ReadAllText(file, Encoding.UTF8);

            _documents.Add(new DocEntry
            {
                Id = $"doc:{Path.GetFileNameWithoutExtension(file)}",
                Title = Path.GetFileNameWithoutExtension(file),
                Content = content
            });
        }
    }
}
