var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Log directory is injected via environment variable so Docker can mount it
var logDir = Environment.GetEnvironmentVariable("LOG_DIR") ?? "/var/logs/easysave";
Directory.CreateDirectory(logDir);

// POST /api/logs — receive one log entry and append it to the daily file
app.MapPost("/api/logs", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var logEntry = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(logEntry))
        return Results.BadRequest(new { error = "Empty log entry" });

    // One file per day regardless of number of users or machines
    var date = DateTime.Now.ToString("yyyy-MM-dd");
    var filePath = Path.Combine(logDir, $"log_{date}.json");

    // Append the raw JSON line — MachineName and UserName are inside the payload
    await File.AppendAllTextAsync(filePath, logEntry + Environment.NewLine);

    return Results.Ok(new { status = "Log saved" });
});

// GET /health — used by Docker Compose health checks
app.MapGet("/health", () => Results.Ok(new { status = "up" }));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");