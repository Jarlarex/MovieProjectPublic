# MovieProjectPublic

Movie Finder is a WPF application for searching movies, viewing details and trailers, and maintaining local liked/watchlist collections.

## Architecture

- **WPF views** for presentation.
- **MovieViewModel** for UI state and pagination.
- **Services** for OMDb and YouTube API access.
- **Repository** for local persistence.
- **SQLite** for a self-contained local database.

The database is created automatically under the user's local application data directory. No .mdb file is required.

## Setup

1. Install Visual Studio with the .NET Framework 4.7.2 desktop development workload.
2. Clone the repository and restore NuGet packages.
3. Add your API keys to App.config for local development: OmdbApiKey and YouTubeApiKey.
4. Build and run the application.

Do not commit real API keys. Desktop applications cannot make API keys completely secret, so provider-side restrictions should also be configured where supported.

## Database

SQLite is initialized automatically on first launch.

The database is stored at:

%LOCALAPPDATA%/MovieProject1/MovieProject.sqlite

Liked and watchlist rows use IMDb ID as their primary key, preventing duplicate entries.

## Testing

The repository includes automated unit tests for core ViewModel behaviour. The GitHub Actions workflow restores, builds, and runs the test suite on Windows.

## Legacy migration

The previous Microsoft Access/Jet implementation was removed. Existing .mdb data is not automatically imported because the original repository did not contain a canonical database/schema from which a safe migration could be verified.
