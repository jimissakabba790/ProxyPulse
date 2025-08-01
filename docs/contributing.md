# Contributing to ProxyPulse

## Getting Started

### Prerequisites

- Windows 10/11 (x64)
- Visual Studio 2022 or later
- .NET 7.0 SDK
- Git
- PowerShell 7+

### Clone and Build

1. Clone the repository:
```powershell
git clone https://github.com/jimissakabba790/ProxyPulse.git
cd ProxyPulse
```

2. Run the orchestration script:
```powershell
./build.ps1 -Target Development
```

This script:
- Restores NuGet packages
- Builds the solution
- Runs tests
- Copies development certificates
- Configures WinDivert driver

### Development Workflow

1. Create a feature branch:
```powershell
git checkout -b feature/my-feature
```

2. Enable development mode:
```powershell
./build.ps1 -Target DevMode
```

3. Start coding with hot reload:
```powershell
dotnet watch run --project src/ProxyPulse.UI
```

## Adding a New Proxy Provider

1. Create a new class implementing `IProxyProvider`:

```csharp
public class MyCustomProvider : IProxyProvider
{
    public string Name => "MyProvider";
    
    public async Task<IEnumerable<ProxyServer>> FetchProxiesAsync(
        ProxyFetchOptions options,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

2. Register in `Startup.cs`:

```csharp
services.AddProxyProvider<MyCustomProvider>();
```

3. Add configuration in `appsettings.json`:

```json
{
  "ProxyProviders": {
    "MyProvider": {
      "ApiKey": "xxx",
      "Endpoint": "https://api.example.com"
    }
  }
}
```

4. Create unit tests:

```csharp
[Fact]
public async Task FetchProxies_ReturnsValidList()
{
    var provider = new MyCustomProvider();
    var proxies = await provider.FetchProxiesAsync(options, default);
    Assert.NotEmpty(proxies);
}
```

## Updating CI Workflow

1. Edit `.github/workflows/ci.yml`
2. Test locally with [act](https://github.com/nektos/act):
```powershell
act -n  # Dry run
act push # Test push workflow
```

3. Common workflow modifications:

```yaml
jobs:
  my-job:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: My Step
        run: ./build.ps1 -Target MyTarget
```

## Pull Request Guidelines

### Branch Naming

- Features: `feature/description`
- Fixes: `fix/issue-description`
- Docs: `docs/topic`
- Release: `release/x.y.z`

### PR Process

1. Create PR from feature branch to `main`
2. Fill out PR template
3. Ensure all checks pass:
   - Build ✅
   - Tests ✅
   - Code coverage ≥80%
   - No security warnings

### Review Criteria

- Clean commit history
- Updated documentation
- Added tests
- No breaking changes
- Follows coding style

## Release Process

1. Update version:
```powershell
./build.ps1 -Target Version -Version x.y.z
```

2. Create release branch:
```powershell
git checkout -b release/x.y.z
```

3. Push and create PR

4. After merge, tag release:
```powershell
git tag -a vx.y.z -m "Version x.y.z"
git push origin vx.y.z
```
