# .NET 10.0 Upgrade Report

## Project target framework modifications

| Project name                                   | Old Target Framework    | New Target Framework         | Commits                   |
|:-----------------------------------------------|:-----------------------:|:----------------------------:|---------------------------|
| norviguet-control-fletes-api\norviguet-control-fletes-api.csproj |   net9.0                | net10.0                       | 34d00913, 48f630bb, dd7af408 |
| norviguet-control-fletes-api.Tests\norviguet-control-fletes-api.Tests.csproj |   net9.0                | net10.0                       | 4269de12, f4dcc8bb |

## NuGet Packages

| Package Name                        | Old Version | New Version | Commit Id                                 |
|:------------------------------------|:-----------:|:-----------:|-------------------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer |   9.0.7     |  10.0.0     | dd7af408                                   |
| Microsoft.AspNetCore.OpenApi        |   9.0.7     |  10.0.0     | dd7af408                                   |
| Microsoft.EntityFrameworkCore       |   9.0.7     |  10.0.0     | dd7af408                                   |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.9    | 10.0.0     | f4dcc8bb                                   |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.7   | 10.0.0     | dd7af408                                   |
| Microsoft.EntityFrameworkCore.Tools |   9.0.7     |  10.0.0     | dd7af408                                   |

## All commits

| Commit ID              | Description                                |
|:-----------------------|:-------------------------------------------|
| 34d00913                | Commit upgrade plan                         |
| 48f630bb                | Update target framework to net10.0 in csproj |
| dd7af408                | Update package versions in norviguet-control-fletes-api.csproj |
| 4269de12                | Update target framework to net10.0 in Tests.csproj |
| f4dcc8bb                | Update EF Core InMemory to v10.0 in Tests.csproj |

## Project feature upgrades

No feature-specific upgrades were required beyond target framework and NuGet package updates.

## Next steps

- Run a full solution build locally and address any compiler or API issues.
- Verify integration and runtime behavior with upgraded packages.
