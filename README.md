# Sanctuary.Census

An unofficial supplement to [Daybreak Game Company's Census API](https://census.daybreakgames.com), which aims to
present up-to-date and more in-depth PlanetSide 2 data. Data retrieval is fully automated, although major PlanetSide 2
updates will often require small changes in order to update certain collections, which can take a small amount of time
to implement.

*Sanctuary.Census is in no way affiliated with nor endorsed by either Daybreak Games Company or Rogue Planet Games.*

Want to chat with developers of all things Census related? Come say hello in the Planetside Community Developers
Discord!

![Discord](https://img.shields.io/discord/1019343142471880775?color=blue&label=Planetside%20Community%20Developers&logo=discord&logoColor=%2302B4FF)

## Getting Started

An instance of Sanctuary.Census can be found at `https://census.lithafalcon.cc/`. It largely provides the same query
interface as the official Census, so ensure you are familiar with using that. I'd recommend reading
[Leonhard's Census Primer](https://github.com/leonhard-s/auraxium/wiki/Census-API-Primer) if you're not.

Jump right in by heading to [https://census.lithafalcon.cc/get/ps2](https://census.lithafalcon.cc/get/ps2) to view the available collections.
Auto-generated API documentation can be found [here](https://census.lithafalcon.cc/api-doc/index.html).

Sanctuary.Census has a (somewhat primitive) 'realtime' event-stream. Events are pushed every five seconds
when operating nominally, but are likely to come in bursts with up to a minute of downtime.

- Connect at `wss://census.lithafalcon.cc/streaming`.
- Compatible with the official Census' event stream.
- Valid event names: `WorldPopulationUpdate`, `MapStateUpdate`.
- Events are mirrored as collections without the `Update` suffix, e.g. https://census.lithafalcon.cc/get/ps2/map_state.
- Append the `c:censusJSON=false` query parameter to the websocket endpoint
(i.e. `wss://census.lithafalcon.cc/streaming?c:censusJSON=false`)
to disable Census JSON (where all tokens are rendered as strings).

> [!TIP]
> Please read the [differences to Census](docs/differences-to-census.md) documentation to get an overview of any
> differences between Sanctuary.Census and the official Census.

### Namespaces

Sanctuary.Census only provides two namespaces:

- `ps2` - data from the current 'live' release of the game.
- `pts` - data from the current 'test' release of the game. The realtime event stream is not available for PTS.

### The `describe` verb

Sanctuary.Census provides some collections, and fields on existing collections, that the official Census does not have.
In order to better understand these collections, the `describe` verb may be used to obtain typing information and
descriptions of any collection's fields.

> GET /describe/&lt;namespace&gt;/&lt;collection&gt;\
> e.g https://census.lithafalcon.cc/describe/ps2/fire_mode_2

## Building and Deployment

To build and run Sanctuary.Census, you'll require the [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
Furthermore, you'll need access to a MongoDB instance. This is simple to [install locally](https://www.mongodb.com/docs/manual/installation/).

If you intend to deploy your own copy, each application is ready to send logging to a [Seq deployment](https://datalust.co/seq),
and [OpenTelemetry](https://opentelemetry.io) tracing/metrics to an OTLP-compatible endpoint (such as Jaeger or the OTEL
Collector). Each application that exposes an API expects to be hosted behind a reverse proxy.

> [!WARNING]
> A large number of adjustments will be required to compile successfully, as I have not open-sourced the server data
> retrieval component.

Sanctuary.Census can be compiled to Docker images using the following commands:

```sh
cd <project>
dotnet publish -c Release --os linux --arch x64 /t:PublishContainer -p ContainerRegistry=<remote>

# The above may fail on certain Linux installs. Try pushing to the local docker registry, and then re-tagging to the remote:
dotnet publish -c Release --os linux --arch x64 /t:PublishContainer 
docker tag <image_name>:latest <remote_registry>/<image_name>:latest
docker push remote_registry>/<image_name>:latest
```

### Solution Architecture

Sanctuary.Census comprises independent services responsible for the following primary tasks:

- Data exposure (e.g. the API).
- Static collection data extraction and collation.
- Realtime data retrieval.

#### Sanctuary.Census.Common

This project contains the collection models, along with other shared services and types, such as JSON converters
and the base database context.

#### Sanctuary.Census.Api

This project is responsible for serving Census REST queries, via an ASP.NET Core Web API.
This encompasses parsing the Census REST query format, building a corresponding query to
the underlying MongoDB database, and converting the result into a JSON model compatible
with expected Census results.

#### Sanctuary.Census.Builder

This service worker project is responsible for transforming data source caches into the collections surfaced by the API.
Built collections are upserted in the underlying database, and the builder also maintains a diffing provider, in order
to show changes to the collections.

The builder uses multiple data source projects - for example, `Sanctuary.Census.ClientData`. Data source projects
contain their specific data models, data retrieval logic and an object inheriting from `IDataCacheService` responsible
for caching their data.

#### Sanctuary.Census.RealtimeHub

This project is responsible for processing realtime data, which includes upserting the database collections and
distributing updates over the EventStream, which includes responsibility for managing the WebSocket connections. It
receives data from the *realtime collectors* via a gRPC service and uses an ASP.NET Core Web API to provide WebSocket
support.

#### Sanctuary.Census.RealtimeCollector

This project contains an independent game server client that is responsible for connecting to a server and retrieving
realtime data. Collected data is sent to the *realtime hub* using a gRPC connection. Collectors work most efficiently
when connecting to only a single server, but have support for synchronously cycling between multiple servers.

## Contributing

Contributions are more than welcome! Please consider opening an issue first to detail your ideas. This gives a
maintainer a chance to pre-approve the work, and reduces the likelihood of two people working on the same feature
simultaneously.
