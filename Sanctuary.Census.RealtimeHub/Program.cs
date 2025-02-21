using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.RealtimeHub.Objects;
using Sanctuary.Census.RealtimeHub.Services;
using Sanctuary.Census.RealtimeHub.Workers;
using Serilog;
using Serilog.Events;
using System.Net;

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
        builder.Services.AddSystemd();
        SetupLogger(builder);

        // Bind kestrel to default protocol for websockets, and HTTP2 for gRPC
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            foreach (string value in context.Configuration["Addresses:WebSocket"]!.Split(";"))
                options.Listen(IPEndPoint.Parse(value));
            foreach (string value in context.Configuration["Addresses:Grpc"]!.Split(";"))
                options.Listen(IPEndPoint.Parse(value), listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
        });

        builder.Services.Configure<CommonOptions>(builder.Configuration.GetSection(nameof(CommonOptions)));
        builder.Services.Configure<BearerTokenAuthenticationOptions>
        (
            builder.Configuration.GetSection(BearerTokenAuthenticationOptions.OptionsName)
        );
        builder.Services.Configure<ZoneConnectionOptions>
        (
            builder.Configuration.GetSection(ZoneConnectionOptions.OPTIONS_NAME)
        );

        builder.AddCommonServices();
        builder.Services.AddSingleton<EventStreamSocketManager>();

        builder.Services.AddAuthentication("BearerTokenAuthentication")
            .AddScheme<BearerTokenAuthenticationOptions, BearerTokenAuthenticationHandler>
            (
                "BearerTokenAuthentication",
                null
            );
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        builder.Services.AddGrpc();

        builder.Services.AddHostedService<RealtimeCollectionPruneWorker>();
        builder.Services.AddHostedService<EventStreamWorker>();

        WebApplication app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseSerilogRequestLogging();
        app.UseWebSockets();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGrpcService<RealtimeIngressService>().RequireAuthorization();

        app.Run();
    }

    private static void SetupLogger(WebApplicationBuilder builder)
    {
        string? seqIngestionEndpoint = builder.Configuration["LoggingOptions:SeqIngestionEndpoint"];
        string? seqApiKey = builder.Configuration["LoggingOptions:SeqApiKey"];

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

        bool useSeq = builder.Environment.IsProduction()
            && !string.IsNullOrEmpty(seqIngestionEndpoint)
            && !string.IsNullOrEmpty(seqApiKey);
        if (useSeq)
        {
            Serilog.Core.LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint!, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Services.AddSerilog();
    }
}
