using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.RealtimeCollector.Services;
using Sanctuary.Census.RealtimeHub;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Extensions;
using Sanctuary.Census.ServerData.Internal.Objects;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeCollector;

/// <summary>
/// The entry class of the application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The entry function of the application.
    /// </summary>
    /// <param name="args">The command line arguments to use.</param>
    public static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSystemd();
        SetupLogger(builder);

        builder.Services.AddOpenTelemetry()
            .WithTracing
            (
                tracerProviderBuilder =>
                {
                    tracerProviderBuilder.ConfigureResource(resourceBuilder => resourceBuilder.AddService
                        (
                            RealtimeCollectorTelemetry.SERVICE_NAME,
                            serviceVersion: RealtimeCollectorTelemetry.SERVICE_VERSION
                        ))
                        .AddServerDataInstrumentation();

                    string? otlpEndpoint = builder.Configuration["LoggingOptions:OtlpEndpoint"];
                    if (otlpEndpoint is null && builder.Environment.IsDevelopment())
                    {
                        tracerProviderBuilder.AddConsoleExporter();
                    }
                    else if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        tracerProviderBuilder.AddOtlpExporter
                        (
                            otlpOptions => otlpOptions.Endpoint = new Uri(otlpEndpoint)
                        );
                    }
                }
            );

        builder.Services.Configure<LoginClientOptions>(builder.Configuration.GetSection(nameof(LoginClientOptions)))
            .Configure<GatewayClientOptions>(builder.Configuration.GetSection(nameof(GatewayClientOptions)))
            .Configure<ContinuousDataOptions>(builder.Configuration.GetSection(nameof(ContinuousDataOptions)));

        builder.Services.AddCommonServices(builder.Environment)
            .AddTransient<IClientDetailsService, HubClientDetailsService>()
            .AddInternalServerDataServices(builder.Environment);

        // Retrieve the hub endpoint from configuration
        string? hubEndpointString = builder.Configuration[$"{CollectorConfig.ConfigName}:{nameof(CollectorConfig.HubEndpoint)}"];
        if (string.IsNullOrEmpty(hubEndpointString))
            throw new InvalidOperationException("Must specify a hub endpoint in config");
        Uri hubEndpoint = new(hubEndpointString);

        // Register the GRPC hub client
        IHttpClientBuilder grpcClient = builder.Services
            .AddGrpcClient<RealtimeIngress.RealtimeIngressClient>(o => o.Address = hubEndpoint)
            .AddCallCredentials((_, metadata) =>
            {
                string? hubToken = builder.Configuration[$"{CollectorConfig.ConfigName}:{nameof(CollectorConfig.HubToken)}"];
                if (!string.IsNullOrEmpty(hubToken))
                    metadata.Add("Authorization", $"Bearer {hubToken}");
                return Task.CompletedTask;
            });

        if (hubEndpoint is { IsLoopback: true, Scheme: "http" })
            grpcClient.ConfigureChannel(o => o.UnsafeUseInsecureChannelCallCredentials = true);

        builder.Services.AddHostedService<ServerConnectionWorker>();

        builder.Build().Run();
    }

    private static void SetupLogger(HostApplicationBuilder builder)
    {
        string? seqIngestionEndpoint = builder.Configuration["LoggingOptions:SeqIngestionEndpoint"];
        string? seqApiKey = builder.Configuration["LoggingOptions:SeqApiKey"];

        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Grpc", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
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
            LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint!, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Services.AddSerilog();
    }
}
