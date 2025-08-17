using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Elasticsearch; // <-- Importante para o sink do ES

var elasticUrl = Environment.GetEnvironmentVariable("ELASTIC_URL") ?? "http://elasticsearch:9200";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
    )
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUrl))
    {
        AutoRegisterTemplate = true,          // Cria automaticamente o template de index no ES
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7, // Ajusta para a versão do ES (se for v8, trocar aqui)
        IndexFormat = "dotnet-logs-{0:yyyy.MM.dd}", // Nome dos índices no Elasticsearch
        NumberOfReplicas = 1,
        NumberOfShards = 1
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

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("DevOpps is the best!");
    logger.LogWarning("This is a warning message.");
    logger.LogError("This is an error message.");
    logger.LogCritical("This is a critical message.");
    logger.LogDebug("This is a debug message.");
    logger.LogTrace("This is a trace message.");
    logger.LogInformation("This is an information message.");
    return "Hello Observability with Elasticsearch!";
});

app.Run();
