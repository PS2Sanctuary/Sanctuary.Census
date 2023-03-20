using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.RealtimeHub;
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
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Early logging and telemetry setup

                SetupLogger(context.Configuration, context.HostingEnvironment);

                services.AddOpenTelemetry()
                    .WithTracing
                    (
                        tracerProviderBuilder =>
                        {
                            tracerProviderBuilder.AddServerDataInstrumentation();

                            string? otlpEndpoint = context.Configuration["LoggingOptions:OtlpEndpoint"];
                            if (otlpEndpoint is null && context.HostingEnvironment.IsDevelopment())
                            {
                                tracerProviderBuilder.AddConsoleExporter();
                            }
                            else if (otlpEndpoint is not null)
                            {
                                tracerProviderBuilder.AddOtlpExporter
                                (
                                    otlpOptions => otlpOptions.Endpoint = new Uri(otlpEndpoint)
                                );
                            }
                        }
                    );
            })
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                services.Configure<LoginClientOptions>(context.Configuration.GetSection(nameof(LoginClientOptions)))
                    .Configure<GatewayClientOptions>(context.Configuration.GetSection(nameof(GatewayClientOptions)))
                    .Configure<ContinuousDataOptions>(context.Configuration.GetSection(nameof(ContinuousDataOptions)));

                services.AddCommonServices(context.HostingEnvironment)
                    .AddInternalServerDataServices(context.HostingEnvironment);

                services.AddGrpcClient<RealtimeIngress.RealtimeIngressClient>(o =>
                    {
                        string? hubEndpoint = context.Configuration[$"{CollectorConfig.ConfigName}:{nameof(CollectorConfig.HubEndpoint)}"];
                        if (string.IsNullOrEmpty(hubEndpoint))
                            throw new InvalidOperationException("Must specify a hub endpoint in config");
                        o.Address = new Uri(hubEndpoint);
                    })
                    .AddCallCredentials((_, metadata) =>
                    {
                        string? hubToken = context.Configuration[$"{CollectorConfig.ConfigName}:{nameof(CollectorConfig.HubToken)}"];
                        if (!string.IsNullOrEmpty(hubToken))
                            metadata.Add("Authorization", $"Bearer {hubToken}");
                        return Task.CompletedTask;
                    });

                services.AddHostedService<ServerConnectionWorker>();
            })
            .Build();

        host.Run();
    }

    private static void SetupLogger(IConfiguration configuration, IHostEnvironment environment)
    {
        string? seqIngestionEndpoint = configuration["LoggingOptions:SeqIngestionEndpoint"];
        string? seqApiKey = configuration["LoggingOptions:SeqApiKey"];

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

        bool useSeq = environment.IsProduction()
            && !string.IsNullOrEmpty(seqIngestionEndpoint)
            && !string.IsNullOrEmpty(seqApiKey);
        if (useSeq)
        {
            LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint!, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }


        Log.Logger = loggerConfig.CreateLogger();
    }
}
