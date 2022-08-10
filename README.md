# Sanctuary.Census

An unofficial supplement to Daybreak Game Company's Census API, which aims to present up-to-date static PlanetSide 2 data.
The methods through which Sanctuary.Census retrieves data allow it to keep itself up-to-date automatically, until
such a time as the structure of the data it depends on, or the methods required to retrieve it, change.

This collection updating process is currently performed every three hours.

## Getting Started

An instance of Sanctuary.Census can be found at `https://census.lithafalcon.cc/`. It largely provides the same query interface
as the official Census, so make sure you're familiar with using it. I'd recommend checking out
[Leonhard's Census Primer](https://github.com/leonhard-s/auraxium/wiki/Census-API-Primer) if you're not.

Jump right in by heading to [https://census.lithafalcon.cc/get/ps2](https://census.lithafalcon.cc/get/ps2) to view the available collections.

⚠ Finally, please read the [migrating from Census](docs/migrating-from-census.md) documentation.

⚠ The API should be considered unstable, although hopefully only in a manner that features are being added in a non-disruptive way.

## Building

To build Sanctuary.Census, you'll require the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
Furthermore, you'll need access to a MongoDB instance. This is simple to [install locally](https://www.mongodb.com/docs/manual/installation/).
Sanctuary.Census expects the database to be running on the default endpoint of `localhost:27017`, and there is currently
no way to configure this.

I'm afraid at the current moment, you'll have to make a large number of adjustments to allow compilation.
This is because I have not open-sourced a particular data retrieval component, and don't have any plans to
do so in the near future.

### Solution Structure

The main project, `Sanctuary.Census`, contains the API, collection models and collection building logic.

Each data source has its own project - for example, `Sanctuary.Census.ClientData`. Data source projects contain their
specific data models, data retrieval logic and an object inheriting from `IDataCacheService` responsible for caching the data, for use in
the main project's collection builders.

Finally, the `Sanctuary.Common` project contains shared data types and services.

## Contributing

Contributions are more than welcome! Please consider opening an issue first to detail your ideas. This gives a maintainer a chance to pre-approve
the work, and reduces the likelihood of two people working on the same feature simultaneously.
