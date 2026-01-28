using Microsoft.AspNetCore.Mvc;
using EnterpriseDataCopilot.Application.Abstractions;
using EnterpriseDataCopilot.Application.Copilot.Ask;

namespace EnterpriseDataCopilot.Api.Controllers;

[ApiController]
[Route("api/copilot")]
public sealed class CopilotController : ControllerBase
{
    [HttpGet("health")]
    public async Task<IActionResult> Health(
        [FromServices] IAuditWriter audit,
        [FromServices] IClock clock,
        CancellationToken ct)
    {
        await audit.WriteAsync(new AuditEntry(
            TimestampUtc: clock.UtcNow,
            EventType: "Health",
            Message: "Copilot API is alive"
        ), ct);

        return Ok(new { status = "ok", utcNow = clock.UtcNow });
    }

    [HttpGet("audit")]
    public async Task<IActionResult> Audit(
        [FromServices] IAuditWriter audit,
        CancellationToken ct)
    {
        var items = await audit.ReadLatestAsync(take: 50, ct);
        return Ok(items);
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
    [FromBody] AskCopilotRequest request,
    [FromServices] AskCopilotHandler handler,
    CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);
        return Ok(result);
    }

}
