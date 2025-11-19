# .NET 10 Upgrade Report

## Project target framework modifications

| Project name                                   | Old Target Framework    | New Target Framework | Commits |
|:-----------------------------------------------|:-----------------------:|:--------------------:|:-------:|
| norviguet-control-fletes-api\norviguet-control-fletes-api.csproj |   net9.0                | net10.0              | -       |
| norviguet-control-fletes-api.Tests\norviguet-control-fletes-api.Tests.csproj |   net9.0                | net10.0              | -       |

## NuGet Packages

| Package Name                        | Old Version | New Version | Commit Id |
|:------------------------------------|:-----------:|:-----------:|:---------:|
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.7      | 10.0.0     | -         |
| Microsoft.AspNetCore.OpenApi        | 9.0.7      | 10.0.0     | -         |
| Microsoft.EntityFrameworkCore       | 9.0.7      | 10.0.0     | -         |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.9   | 10.0.0     | -         |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.7  | 10.0.0     | -         |
| Microsoft.EntityFrameworkCore.Tools | 9.0.7     | 10.0.0     | -         |

## Project feature upgrades

- Initial upgrade completed for both projects: target frameworks and package references updated to `net10.0` and package versions updated where applicable.
- Validations performed: SDK presence, global.json check, project validation and build. Build succeeded.

## Next steps

- Run unit tests and integration tests; review test results and fix any failures.
- Manual validation of authentication flows and EF Core behavior in runtime environments.

## All commits

- No commits detected by change scanner.

## Tokens and cost

- Tokens used: N/A
- Cost estimate: N/A
