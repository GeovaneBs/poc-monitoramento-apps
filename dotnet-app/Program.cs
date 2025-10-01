using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Grafana.Loki;

var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL") ?? "http://loki:3100";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .Enrich.WithProperty("Application", "poc-dotnet")
    .Enrich.WithProperty("Environment", "development")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(lokiUrl, labels: new[]
    {
        new LokiLabel { Key = "app", Value = "poc-dotnet" },
        new LokiLabel { Key = "env", Value = "development" }
    })
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Usa o Serilog como provider de logging
builder.Host.UseSerilog();

// Configuração do OpenTelemetry (mantém como estava)
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("poc-dotnet"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://jaeger:4317");
        })
    );

var app = builder.Build();

app.MapGet("/", async (ILogger<Program> logger, HttpContext context) =>
{
    using var activity = Activity.Current;
    activity?.SetTag("http.route", "/");
    activity?.SetTag("user.id", new Random().Next(1000, 9999));

    logger.LogInformation("Request received on root endpoint from {UserAgent}",
        context.Request.Headers.UserAgent.ToString());

    // Simula processamento
    await Task.Delay(Random.Shared.Next(10, 100));

    return "Hello Observability with Loki + Jaeger!";
});

app.MapGet("/process", async (ILogger<Program> logger) =>
{
    using var activity = Activity.Current;
    activity?.SetTag("operation.type", "process");

    var orderId = Random.Shared.Next(10000, 99999);
    activity?.SetTag("order.id", orderId);

    logger.LogInformation("Processing order {OrderId}", orderId);

    // Simula processamento com delay
    await Task.Delay(Random.Shared.Next(100, 500));

    logger.LogInformation("Order {OrderId} processed successfully", orderId);
    return new { orderId, status = "processed" };
});

// Task em background para simular logs automáticos
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var backgroundLogger = loggerFactory.CreateLogger("BackgroundSimulator");

_ = Task.Run(async () =>
{
    var random = new Random();
    var operations = new[] { "ProcessOrder", "UpdateInventory", "SendEmail", "ValidateUser", "CalculateDiscount" };
    var statuses = new[] { "Success", "Failed", "Timeout", "Retry" };

    while (true)
    {
        try
        {
            var operation = operations[random.Next(operations.Length)];
            var status = statuses[random.Next(statuses.Length)];
            var responseTime = random.Next(50, 2000);

            var logLevel = status switch
            {
                "Success" => LogLevel.Information,
                "Failed" => LogLevel.Error,
                "Timeout" => LogLevel.Warning,
                "Retry" => LogLevel.Warning,
                _ => LogLevel.Information
            };

            backgroundLogger.Log(logLevel,
                "Operation: {Operation} | Status: {Status} | ResponseTime: {ResponseTime}ms | UserId: {UserId}",
                operation, status, responseTime, random.Next(1000, 9999));

            // Varia o intervalo entre 1 a 5 segundos
            await Task.Delay(random.Next(1000, 5000));
        }
        catch (Exception ex)
        {
            backgroundLogger.LogError(ex, "Error in background log simulator");
            await Task.Delay(5000);
        }
    }
});

app.Run();
