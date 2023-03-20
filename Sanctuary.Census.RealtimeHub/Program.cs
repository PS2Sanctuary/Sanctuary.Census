using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.RealtimeHub.Services;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.RealtimeHub;

/// <summary>
/// The entry class of the application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The entry function of the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSystemd();

        SetupLogger(builder.Configuration, builder.Environment);
        builder.Host.UseSerilog();

        builder.Services.Configure<CommonOptions>(builder.Configuration.GetSection(nameof(CommonOptions)));

        builder.Services.AddCommonServices(builder.Environment);

        builder.Services.AddGrpc();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy();
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });


        builder.Services.AddHostedService<RealtimeCollectionPruneWorker>();

        WebApplication app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseSerilogRequestLogging();
        app.MapGrpcService<RealtimeIngressService>();
        app.MapControllers();

        app.Run();
    }

    private static void SetupLogger(IConfiguration configuration, IHostEnvironment environment)
    {
        string? seqIngestionEndpoint = configuration["LoggingOptions:SeqIngestionEndpoint"];
        string? seqApiKey = configuration["LoggingOptions:SeqApiKey"];

        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Grpc", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console
            (
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            );

        bool useSeq = environment.IsProduction()
            && !string.IsNullOrEmpty(seqIngestionEndpoint)
            && !string.IsNullOrEmpty(seqApiKey);
        if (useSeq)
        {
            Serilog.Core.LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint!, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }

        Log.Logger = loggerConfig.CreateLogger();
    }
}
