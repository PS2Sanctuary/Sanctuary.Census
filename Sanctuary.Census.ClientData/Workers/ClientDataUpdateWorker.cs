using Microsoft.Extensions.Hosting;
using Sanctuary.Census.ClientData.Abstractions.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Workers;

public class ClientDataUpdateWorker : BackgroundService
{
    private readonly IManifestService _manifestService;

    public ClientDataUpdateWorker(IManifestService manifestService)
    {
        _manifestService = manifestService;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            
            
            await Task.Delay(TimeSpan.FromMinutes(15), ct).ConfigureAwait(false);
        }
    }
}
