using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Sanctuary.Census.Api.Controllers;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Api.Exceptions;
using Sanctuary.Census.Api.Json;
using Sanctuary.Census.Api.Middleware;
using Sanctuary.Census.Api.Models;
using Sanctuary.Census.Api.Services;
using Sanctuary.Census.Common.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sanctuary.Census.Api;

/// <summary>
/// The main class of the application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">Runtime arguments to be passed to the application.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSystemd();
        SetupLogger(builder);

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metricsProviderBuilder =>
            {
                metricsProviderBuilder.ConfigureResource(resourceBuilder =>
                {
                    resourceBuilder.AddService(ApiTelemetry.SERVICE_NAME, serviceVersion: ApiTelemetry.SERVICE_VERSION);
                });
                metricsProviderBuilder.AddMeter(ApiTelemetry.Meter.Name);

                string? otlpEndpoint = builder.Configuration["LoggingOptions:OtlpEndpoint"];
                if (string.IsNullOrEmpty(otlpEndpoint) && builder.Environment.IsDevelopment())
                {
                    metricsProviderBuilder.AddConsoleExporter();
                }
                else if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metricsProviderBuilder.AddOtlpExporter
                    (
                        otlpOptions => otlpOptions.Endpoint = new Uri(otlpEndpoint)
                    );
                }
            });

        builder.Services.Configure<CommonOptions>(builder.Configuration.GetSection(nameof(CommonOptions)));

        builder.Services.AddCommonServices(builder.Environment)
            .AddSingleton<CollectionDescriptionService>();

        builder.Services.AddCors()
            .AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new BooleanModelBinderProvider());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy();
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new BsonDocumentJsonConverter(false));
                options.JsonSerializerOptions.Converters.Add(new DataResponseJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new BsonDecimal128JsonConverter());
            });

        string? exposeDiffInterfaceStr = builder.Configuration["ApiOptions:ExposeDiffInterface"];
        if (bool.TryParse(exposeDiffInterfaceStr, out bool exposeDiffInterface) && exposeDiffInterface)
            builder.Services.AddRazorPages();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.DocumentFilter<CollectionModelDocumentFilter>();
            options.SchemaFilter<EnumSchemaFilter>();
            options.ParameterFilter<EnvironmentParameterFilter>();

            IEnumerable<Assembly> assems = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.FullName?.StartsWith("Sanctuary.Census") == true);

            foreach (Assembly a in assems)
            {
                string xmlFilename = $"{a.GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            }
        });

        WebApplication app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseSerilogRequestLogging();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    IExceptionHandlerPathFeature? exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                    if (exceptionHandlerPathFeature?.Error is QueryException quex)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        ErrorResponse er = new(quex.ErrorCode, quex.Message);
                        await context.Response.WriteAsJsonAsync(er);
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        ErrorResponse er = new(QueryErrorCode.ServerError, string.Empty);
                        await context.Response.WriteAsJsonAsync(er);
                    }
                });
            });
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = "api-doc";
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseMiddleware<ServiceIDMiddleware>();

        app.MapGet("/", [ApiExplorerSettings(IgnoreApi = true)](c) =>
        {
            c.Response.Redirect("https://github.com/carlst99/Sanctuary.Census");
            return Task.CompletedTask;
        });

        app.UseRouting();
        app.UseCors
        (
            x => x.WithMethods(HttpMethods.Get)
                .AllowAnyHeader()
                .AllowAnyOrigin()
        );
        app.UseAuthorization();
        app.MapControllers();

        if (exposeDiffInterface)
            app.MapRazorPages();

        app.Run();
    }

    private static void SetupLogger(WebApplicationBuilder builder)
    {
        string? seqIngestionEndpoint = builder.Configuration["LoggingOptions:SeqIngestionEndpoint"];
        string? seqApiKey = builder.Configuration["LoggingOptions:SeqApiKey"];

        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
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
