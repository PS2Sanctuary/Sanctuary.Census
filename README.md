# Sanctuary.Census

An unofficial supplement for Daybreak Game Company's Census API, which uses alternative data sources to present up-to-date PlanetSide 2 data.
Sanctuary.Census pulls data from both client files and the game servers, using automated processes for both.

## Getting Started

An instance of Sanctuary.Census can be found at [https://census.lithafalcon.cc/](https://census.lithafalcon.cc/). It uses the same query structure
(or at least, hopes to eventually) as the official Census, so make sure you're familiar with using it.

Jump right in by heading to [https://census.lithafalcon.cc/get/ps2](https://census.lithafalcon.cc/get/ps2) to view the available collections.

⚠ Finally, please read the [migrating from Census](docs/migrating-from-census.md) documentation.

⚠ The API should be considered unstable, although hopefully only in a manner that features are being added in a non-disruptive way.

## Roadmap

I'd really like to get my hands on facility and map region data, and although I'm actively looking it's proving elusive.
On a positive note, I think I might have found some degree of directive and marketing bundle data. Hopefully I can add that soon.

At some point, I'll be looking to properly replicate Census' query interface. However, it's not high on my to-do list.

## Contributing

Contributions are more than welcome! However, I'm afraid at the current moment, you'll have to make a large number of adjustments to allow compilation.
This is because I have not open-sourced the server connection logic, and don't have any plans to do so in the near future.

If you'd still like to go ahead with contributing, consider opening an issue first to detail your ideas. This gives a maintainer a chance to pre-approve
the work, and reduces the likelihood of two people working on the same feature simultaneously.

### Structure

The main project, `Sanctuary.Census`, contains the API, collection models and collection building logic.

Each data source has its own project. The only visible one at the moment is `Sanctuary.Census.ClientData`. Data source projects contain their
specific data models, data retrieval logic and an object inheriting from `IDataCacheService` responsible for caching the data, for use in
the main project's collection builders.

Finally, the `Sanctuary.Common` project contains shared data types and services.
