using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Abstractions.Services;
using Sanctuary.Census.ClientData.Extensions;
using Sanctuary.Census.CollectionBuilders;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Database;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Json;
using Sanctuary.Census.Middleware;
using Sanctuary.Census.Models;
using Sanctuary.Census.PatchData.Extensions;
using Sanctuary.Census.ServerData.Internal.Extensions;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Census.Services;
using Sanctuary.Census.Workers;
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

namespace Sanctuary.Census;

/// <summary>
/// The main class of the application.
/// </summary>
public static class Program
{
    private const string LOGGING_OPTIONS_NAME = "LoggingOptions";

    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">Runtime arguments to be passed to the application.</param>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        if (!builder.Environment.IsDevelopment() && OperatingSystem.IsLinux())
            builder.Host.UseSystemd();

        string? seqIngestionEndpoint = builder.Configuration.GetSection(LOGGING_OPTIONS_NAME).GetSection("SeqIngestionEndpoint").Value;
        string? seqApiKey = builder.Configuration.GetSection(LOGGING_OPTIONS_NAME).GetSection("SeqApiKey").Value;
        SetupLogger(seqIngestionEndpoint, seqApiKey);
        builder.Host.UseSerilog();

        builder.Services.Configure<CommonOptions>(builder.Configuration.GetSection(nameof(CommonOptions)))
            .Configure<LoginClientOptions>(builder.Configuration.GetSection(nameof(LoginClientOptions)))
            .Configure<GatewayClientOptions>(builder.Configuration.GetSection(nameof(GatewayClientOptions)));

        builder.Services.AddMemoryCache()
            .AddClientDataServices()
            .AddInternalServerDataServices()
            .AddPatchDataServices();

        builder.Services.AddSingleton(new MongoClient("mongodb://localhost:27017"))
            .AddScoped<IMongoContext, MongoContext>()
            .RegisterCollectionBuilders()
            .AddHostedService<CollectionBuildWorker>();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy();
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new DataResponseJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new BsonDocumentJsonConverter());
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
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
        app.UseMiddleware<ServiceIDMiddleware>();

        app.MapGet("/", [ApiExplorerSettings(IgnoreApi = true)](c) =>
        {
            c.Response.Redirect("https://github.com/carlst99/Sanctuary.Census");
            return Task.CompletedTask;
        });

        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }

    private static IServiceCollection RegisterCollectionBuilders(this IServiceCollection services)
    {
        services.AddSingleton<ICollectionBuilderRepository>
        (
            s => s.GetRequiredService<IOptions<CollectionBuilderRepository>>().Value
        );

        return services.RegisterCollectionBuilder<CurrencyCollectionBuilder>()
            .RegisterCollectionBuilder<ExperienceCollectionBuilder>()
            .RegisterCollectionBuilder<FacilityLinkCollectionBuilder>()
            .RegisterCollectionBuilder<FactionCollectionBuilder>()
            .RegisterCollectionBuilder<FireGroupCollectionBuilder>()
            .RegisterCollectionBuilder<FireGroupToFireModeCollectionBuilder>()
            .RegisterCollectionBuilder<FireModeCollectionBuilder>()
            .RegisterCollectionBuilder<FireModeToProjectileCollectionBuilder>()
            .RegisterCollectionBuilder<ItemCollectionBuilder>()
            .RegisterCollectionBuilder<ItemCategoryCollectionBuilder>()
            .RegisterCollectionBuilder<ItemToWeaponCollectionBuilder>()
            .RegisterCollectionBuilder<LoadoutSlotCollectionBuilder>()
            .RegisterCollectionBuilder<MapRegionCollectionBuilder>()
            .RegisterCollectionBuilder<PlayerStateGroup2CollectionBuilder>()
            .RegisterCollectionBuilder<ProfileCollectionBuilder>()
            .RegisterCollectionBuilder<ProjectileCollectionBuilder>()
            .RegisterCollectionBuilder<VehicleCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponAmmoSlotCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponToFireGroupCollectionBuilder>()
            .RegisterCollectionBuilder<WorldCollectionBuilder>();
    }

    private static IServiceCollection RegisterCollectionBuilder<TBuilder>(this IServiceCollection services)
        where TBuilder : class, ICollectionBuilder
    {
        services.AddScoped<TBuilder>();
        services.Configure<CollectionBuilderRepository>(x => x.Register<TBuilder>());
        return services;
    }

    // ReSharper disable twice UnusedParameter.Local
    private static void SetupLogger(string? seqIngestionEndpoint, string? seqApiKey)
    {
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

#if !DEBUG
        if (!string.IsNullOrEmpty(seqIngestionEndpoint) && !string.IsNullOrEmpty(seqApiKey))
        {
            Serilog.Core.LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }
#endif

        Log.Logger = loggerConfig.CreateLogger();
    }
}
