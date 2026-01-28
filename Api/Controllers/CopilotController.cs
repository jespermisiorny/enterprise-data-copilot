using Api.Contracts;
using Api.Audit;
using Api.Docs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/copilot")]
public class CopilotController : ControllerBase
{
    private readonly InMemoryAuditStore _auditStore;
    private readonly DocumentLoader _documentLoader;


    public CopilotController(InMemoryAuditStore auditStore, DocumentLoader documentLoader)
    {
        _auditStore = auditStore;
        _documentLoader = documentLoader;
    }

    [HttpPost("query")]
    public ActionResult<CopilotResponse> Query([FromBody] CopilotQueryRequest req)
    {
        var q = (req.Text ?? string.Empty).ToLowerInvariant();
        var allDocs = _documentLoader.GetAll();

        var relevantDocs = new List<DocEntry>();

        // Mycket enkel "matchning" för MVP
        if (q.Contains("kostnad") || q.Contains("kostnader") || q.Contains("budget"))
        {
            relevantDocs.AddRange(allDocs.Where(d => d.Id.Contains("kpi", StringComparison.OrdinalIgnoreCase)));
        }

        if (
            q.Contains("kostnadsställe") ||
            q.Contains("produktion") ||
            q.Contains("tidsdimension") ||
            q.Contains("tid") ||
            q.Contains("vecka") ||
            q.Contains("månad") ||
            q.Contains("kvartal") ||
            q.Contains("år")
        )
        {
            relevantDocs.AddRange(
                allDocs.Where(d => d.Id.Contains("data", StringComparison.OrdinalIgnoreCase))
            );
        }


        // ta bort dubletter (om samma doc råkar läggas till två gånger)
        relevantDocs = relevantDocs
            .GroupBy(d => d.Id)
            .Select(g => g.First())
            .ToList();

        var citations = relevantDocs.Select(d =>
                new CopilotCitation(
            Id: d.Id,
            Label: d.Title,
            Snippet: d.Content.Length > 220
                ? d.Content.Substring(0, 220) + "..."
                : d.Content
                )
            ).ToArray();

        var evidence = relevantDocs.Select(d => d.Id).ToArray();

        var response = new CopilotResponse(
Summary: citations.Length > 0
    ? $"Hittade {citations.Length} relevanta källor och genererade insikter baserat på dessa."
    : "Jag hittade inga matchande källor för den frågan. Prova att vara mer specifik eller ställ en följdfråga.",

            Confidence: "medium",
            Insights: new[]
            {
                new CopilotInsight(
                    Title: "Kostnader ökade 12% i Produktion",
                    Type: "anomaly",
                    Metric: "TotalCost",
                    Delta: 0.12,
                    Why: "Övertid och externa resurser är vanliga drivare enligt KPI-definitionerna.",
                    Evidence: evidence.Length > 0 ? evidence : new[] { "doc:inga-källor" }
                ),
                new CopilotInsight(
                    Title: "Budgetavvikelse främst i Kostnadsställe 310",
                    Type: "explanation",
                    Metric: "BudgetVariance",
                    Delta: 0.08,
                    Why: "Tidigarelagda investeringar kan skapa positiv budgetavvikelse enligt glossary.",
                    Evidence: evidence.Length > 0 ? evidence : new[] { "doc:inga-källor" }
                )
            },
            Citations: citations,
            FollowUps: new[]
            {
                "Vill du se detta per vecka?",
                "Vilka leverantörer stod för ökningen?",
                "Jämför mot samma period förra året?"
            }
        );

        _auditStore.Add(new AuditEntry
        {
            Question = req.Text,
            Summary = response.Summary,
            Confidence = response.Confidence
        });

        return Ok(response);
    }

    [HttpGet("audit")]
    public ActionResult<IEnumerable<AuditEntry>> GetAuditLog()
    {
        return Ok(_auditStore.GetAll());
    }

}
