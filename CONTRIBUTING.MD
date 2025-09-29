# Contributing to Rydia

## Database

- Entity Framework Core migrations are stored in `src/Rydia.Database/Migrations` and include the full Quartz persistent store schema.
- On startup, Rydia automatically applies any pending migrations to the database configured in `appsettings.Development.json` (or the active environment).
- This means a fresh install only requires providing database credentials; the schema is created and kept up to date without running SQL scripts manually.
