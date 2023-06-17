using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.RealtimeHub.Services;
using Sanctuary.Census.RealtimeHub.Workers;
using Serilog;
using Serilog.Events;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebSocketManager = Sanctuary.Census.RealtimeHub.Services.WebSocketManager;

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
        builder.Services.AddSingleton<WebSocketManager>();

        builder.Services.AddGrpc();

        builder.Services.AddHostedService<RealtimeCollectionPruneWorker>();
        builder.Services.AddHostedService<WebSocketWorker>();

        WebApplication app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseSerilogRequestLogging();
        app.UseWebSockets();

        app.MapGrpcService<RealtimeIngressService>();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path != "/eventstream")
            {
                await next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            TaskCompletionSource<object> socketFinishedTcs = new();

            context.RequestServices.GetRequiredService<WebSocketManager>()
                .AddWebSocket(webSocket, socketFinishedTcs);
            await socketFinishedTcs.Task;
        });

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
