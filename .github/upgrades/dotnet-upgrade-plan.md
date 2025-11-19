# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade norviguet-control-fletes-api\norviguet-control-fletes-api.csproj
4. Upgrade norviguet-control-fletes-api.Tests\norviguet-control-fletes-api.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                | Current Version | New Version | Description                                   |
|:--------------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| Microsoft.AspNetCore.Authentication.JwtBearer |     9.0.7      |   10.0.0   | Replace with Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0 for .NET 10.0 |
| Microsoft.AspNetCore.OpenApi                 |     9.0.7      |   10.0.0   | Replace with Microsoft.AspNetCore.OpenApi 10.0.0 for .NET 10.0 |
| Microsoft.EntityFrameworkCore                |     9.0.7      |   10.0.0   | Replace with Microsoft.EntityFrameworkCore 10.0.0 for .NET 10.0 |
| Microsoft.EntityFrameworkCore.InMemory       |     9.0.9      |   10.0.0   | Replace with Microsoft.EntityFrameworkCore.InMemory 10.0.0 for .NET 10.0 |
| Microsoft.EntityFrameworkCore.SqlServer      |     9.0.7      |   10.0.0   | Replace with Microsoft.EntityFrameworkCore.SqlServer 10.0.0 for .NET 10.0 |
| Microsoft.EntityFrameworkCore.Tools          |     9.0.7      |   10.0.0   | Replace with Microsoft.EntityFrameworkCore.Tools 10.0.0 for .NET 10.0 |

### Project upgrade details

#### norviguet-control-fletes-api\norviguet-control-fletes-api.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Authentication.JwtBearer should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.AspNetCore.OpenApi should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.EntityFrameworkCore should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.EntityFrameworkCore.SqlServer should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.EntityFrameworkCore.Tools should be updated from `9.0.7` to `10.0.0` (recommended for .NET 10.0)

Feature upgrades:
  - No specific feature migrations detected beyond package and target framework updates.

Other changes:
  - Review code for any APIs deprecated in .NET 10.0 after package upgrades.

#### norviguet-control-fletes-api.Tests\norviguet-control-fletes-api.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore.InMemory should be updated from `9.0.9` to `10.0.0` (recommended for .NET 10.0)

Other changes:
  - Run tests after upgrade to validate behavior.
