using EnterpriseDataCopilot.Application.Abstractions;
using EnterpriseDataCopilot.Application.Copilot.Time;
using EnterpriseDataCopilot.Infrastructure.Audit;
using EnterpriseDataCopilot.Infrastructure.Time;
using EnterpriseDataCopilot.Application.Copilot.Ask;


var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Insaight-grund: tid + audit
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<ITimeContextResolver, TimeContextResolver>();

// Copilot
builder.Services.AddSingleton<AskCopilotHandler>();

// Filbaserad audit (rekommenderas)
var auditPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "audit.jsonl");
builder.Services.AddSingleton<IAuditWriter>(_ => new FileAuditWriter(auditPath));

// CORS (behåll om du har webben på 5173)
const string CorsPolicyName = "WebApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Om du kör Vite/dev på 5173 och vill kunna anropa API:t
app.UseCors(CorsPolicyName);

// Valfritt: kan vara kvar, men om du bara kör http lokalt spelar det mindre roll
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
