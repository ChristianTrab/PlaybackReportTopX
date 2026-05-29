# Jellyfin Playback Reporting Top X Plugin

This plugin creates and maintains collections of the most watched movies and TV series on your Jellyfin server, using playback data collected by the [Playback Reporting](https://github.com/jellyfin/jellyfin-plugin-playbackreporting) plugin.

## Features

- Creates a scheduled task that runs daily at 2:00 AM
- Reads watch history from the Playback Reporting SQLite database
- Identifies the top movies and series within a configurable time period (default: last 30 days)
- Creates separate collections named **Top 10 Movies** and **Top 10 Series** (both configurable)
- For movies, ranks by total play count (tie-breaker: total watch duration)
- For series, aggregates episode plays to their parent series and ranks by total play count
- Series collections store **series** as members; Jellyfin may still display a much larger item count because it recursively includes episodes inside those series
- Keeps collections up to date by replacing items that have fallen out of the top list
- Validates that Playback Reporting is installed and active before updating collections
- Shows a warning banner on the plugin settings page when Playback Reporting is missing, inactive, or not initialized

## Prerequisites

- Jellyfin **10.11** or newer (built and tested against **10.11.9**)
- The [Playback Reporting](https://github.com/jellyfin/jellyfin-plugin-playbackreporting) plugin installed, enabled, and collecting data

## Install Process

1. In Jellyfin, go to `Dashboard -> Plugins -> Catalog -> Gear Icon (upper left)` and add a repository.
1. Set the Repository name to `@ChristianTrab (Playback Reporting Top X)`
1. Set the Repository URL to `https://raw.githubusercontent.com/ChristianTrab/PlaybackReportTopX/refs/heads/main/manifest.json`
1. Click **Save**
1. Go to **Catalog** and search for **Playback Reporting Top X**
1. Click on it and install
1. Restart Jellyfin

You can also install from a GitHub release zip or build from source (see [Building from Source](#building-from-source)).

## User Guide

1. Ensure the **Playback Reporting** plugin is installed and enabled
1. Go to `Dashboard -> Plugins -> My Plugins -> Playback Reporting Top X -> Settings`
1. If Playback Reporting is missing, inactive, or has not created its database yet, a red warning banner appears at the top of the settings page
1. Configure your preferences and click **Save**
1. Go to `Dashboard -> Scheduled Tasks` and run **Update Playback Reporting Top X Collections**
1. Your **Top 10 Movies** and **Top 10 Series** collections should now appear in your library

The scheduled task also runs automatically every day at 2:00 AM. If Playback Reporting is not ready, the task logs a warning and skips the update until the dependency is available.

### Series collection item counts

The plugin adds **series** to the series collection, not individual episodes. If the log says `Updated collection "Top 10 Series" with 6 series`, the collection contains six shows.

Jellyfin may still report a much higher number (for example 260 items) when you browse or inspect the collection, because Jellyfin recursively counts episodes inside those series. That behavior comes from Jellyfin itself, not from extra items being added by this plugin.

If fewer than your configured top count are added, only that many series had matching playback activity in the selected time window and could be mapped to library series.

## Configuration

All settings can be changed from `Dashboard -> Plugins -> My Plugins -> Playback Reporting Top X -> Settings`.

| Settings page label | Config key | Default | Limits | Description |
| --- | --- | --- | --- | --- |
| Number of items per collection | `TopCount` | `10` | 1–100 | Items included in each collection |
| Days of playback history | `DaysToLookBack` | `30` | 1–3650 | Only count plays within this many days |
| Movies collection name | `MoviesCollectionName` | `Top 10 Movies` | 256 characters | Name of the movies collection |
| Series collection name | `SeriesCollectionName` | `Top 10 Series` | 256 characters | Name of the series collection |
| Update movies collection | `UpdateMoviesCollection` | enabled | — | Whether to update the movies collection |
| Update series collection | `UpdateSeriesCollection` | enabled | — | Whether to update the series collection |

Values outside the allowed limits are clamped automatically on save and when the scheduled task runs.

## Playback Reporting dependency

This plugin depends on Playback Reporting at three levels:

1. **Plugin installed** — the Playback Reporting plugin package must be present
2. **Plugin active** — Playback Reporting must be enabled and loaded successfully
3. **Database initialized** — `playback_reporting.db` must exist in Jellyfin's data directory (play some media or open Playback Reporting once after install)

If any check fails:

- The settings page shows a warning banner with details
- The scheduled task skips collection updates and logs the reason
- Jellyfin logs a warning on server startup

## Building from Source

1. Clone this repository
2. Install the [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
3. Build the solution:

   ```bash
   dotnet build Jellyfin.Plugin.PlaybackReportingTopX.sln -c Release -p:TreatWarningsAsErrors=true
   ```

4. Run tests (optional):

   ```bash
   dotnet test Jellyfin.Plugin.PlaybackReportingTopX.Tests/Jellyfin.Plugin.PlaybackReportingTopX.Tests.csproj -c Release
   ```

5. Copy the built DLLs from `Jellyfin.Plugin.PlaybackReportingTopX/bin/Release/net9.0/` into your Jellyfin plugins folder:
   - `Jellyfin.Plugin.PlaybackReportingTopX.dll`
   - `Jellyfin.Plugin.PlaybackReportingTopX.Core.dll`
   - `Microsoft.Data.Sqlite.dll` (and any other dependency DLLs in that folder)

6. Restart Jellyfin

## License

See repository license file.
