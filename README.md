# Sanctuary.Census

An unofficial supplement to [Daybreak Game Company's Census API](https://census.daybreakgames.com), which aims to present up-to-date
static PlanetSide 2 data. The methods through which Sanctuary.Census retrieves data allow it to keep itself up-to-date (mostly) automatically,
until such a time as the structure of the data it depends on, or the methods required to retrieve it, change. Major PlanetSide 2 updates will
often require small changes in order to update certain collections, which can take a small amount of time to implement.

Want to chat with developers of all things Census related? Come say hello in the Planetside Community Developers Discord!\
![Discord](https://img.shields.io/discord/1019343142471880775?color=blue&label=Planetside%20Community%20Developers&logo=discord&logoColor=%2302B4FF)

## Getting Started

An instance of Sanctuary.Census can be found at `https://census.lithafalcon.cc/`. It largely provides the same query interface
as the official Census, so ensure you are familiar with using that. I'd recommend reading
[Leonhard's Census Primer](https://github.com/leonhard-s/auraxium/wiki/Census-API-Primer) if you're not.

Jump right in by heading to [https://census.lithafalcon.cc/get/ps2](https://census.lithafalcon.cc/get/ps2) to view the available collections.

> **Warning**:
> Please read the [migrating from Census](docs/migrating-from-census.md) documentation to get an overview of any differences between
> Sanctuary.Census and the official Census.

### Namespaces

Sanctuary.Census only provides two namespaces:

- `ps2` - data from the current 'live' release of the game.
- `pts` - data from the current 'test' release of the game.

### The `describe` verb

Sanctuary.Census provides some collections, and fields on existing collections, that the official Census does not have.
In order to better understand these collections, the `describe` verb may be used to obtain typing information and
descriptions of the collection's fields.

> GET /describe/&lt;namespace&gt;/&lt;collection&gt;\
> e.g https://census.lithafalcon.cc/describe/ps2/fire_mode_2

## Building

To build Sanctuary.Census, you'll require the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).
Furthermore, you'll need access to a MongoDB instance. This is simple to [install locally](https://www.mongodb.com/docs/manual/installation/).
Sanctuary.Census expects the database to be running on the default endpoint of `localhost:27017`, and there is currently
no way to configure this.

I'm afraid at the current moment, you'll have to make a large number of adjustments to allow compilation.
This is because I have not open-sourced a particular data retrieval component, and don't have any plans to
do so in the near future.

### Solution Structure

#### Sanctuary.Census.Common

This project contains the collection models, along with other shared services and types.

#### Sanctuary.Census.Api

This ASP.NET Core Web API project contains the API components, much as the name suggests.
This encompasses parsing the Census REST query format, building a corresponding query to
the underlying MongoDB database, and converting the result into a JSON model compatible
with expected Census results.

#### Sanctuary.Census.Builder

This service worker project is responsible for transforming data source caches into the
collections surfaced by the API. Built collections are upserted in the underlying database,
and the builder also maintains a diffing provider, in order to show changes to the collections.

The builder uses multiple data source projects - for example, `Sanctuary.Census.ClientData`. Data source projects contain their
specific data models, data retrieval logic and an object inheriting from `IDataCacheService` responsible for caching their data.

## Contributing

Contributions are more than welcome! Please consider opening an issue first to detail your ideas. This gives a maintainer a chance
to pre-approve the work, and reduces the likelihood of two people working on the same feature simultaneously.
