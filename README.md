# ProxyPulse

[![Build Status](https://github.com/jimissakabba790/ProxyPulse/workflows/ProxyPulse%20CI%2FCD/badge.svg)](https://github.com/jimissakabba790/ProxyPulse/actions)
[![CodeQL](https://github.com/jimissakabba790/ProxyPulse/workflows/CodeQL/badge.svg)](https://github.com/jimissakabba790/ProxyPulse/security/code-scanning)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-v1.0.0-blue.svg)](https://github.com/jimissakabba790/ProxyPulse/releases/tag/v1.0.0)

ProxyPulse is a Windows application that enables per-application SOCKS5 proxy routing with real-time proxy endpoint updates. It provides a seamless way to route specific applications through different proxy servers while maintaining high availability through automatic proxy rotation and health checks.

## Architecture

```ascii
[User] → [ProxyPulse.UI] → [Fetchers Plugin] ↔ [Proxy APIs]
                              ↓
                         [SocksCore]
                              ↓
                        [RoutingService]
                              ↓
                    [WinDivert / Proxifier]
                              ↓
                          [Applications]
```

## Prerequisites

- Windows 10 or Windows 11
- No additional prerequisites - ProxyPulse is distributed as a self-contained executable

## Quick Start

```bash
# Download and Install

Download the latest release installer:
https://github.com/jimissakabba790/ProxyPulse/releases/tag/v1.0.0

Or build from source:

```bash
# Clone the repository
git clone https://github.com/jimissakabba790/ProxyPulse.git

# Navigate to the project directory
cd ProxyPulse

# Build the solution
dotnet build

# Run the application
cd src/ProxyPulse.UI
dotnet run
```
```

## Testing & Benchmarks

The project includes comprehensive testing and performance measurement:

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate coverage report
dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Running Benchmarks
```bash
# Build and run benchmarks
dotnet run -c Release -p src/ProxyPulse.Benchmarks/ProxyPulse.Benchmarks.csproj
```

## Next Steps

The project is organized into several modules, each handling a specific aspect of the proxy routing system:

1. **Implement Fetchers** in `ProxyPulse.Fetchers`
   - Add proxy provider implementations
   - Handle API authentication and rate limiting
   - Implement health checks and failover

2. **Wire up SocksCore** in `ProxyPulse.SocksCore`
   - Configure SOCKS5 proxy server
   - Manage proxy endpoint rotation
   - Handle authentication and connection pooling

3. **Configure Routing** in `ProxyPulse.Routing`
   - Set up WinDivert driver installation
   - Implement Proxifier fallback mechanism
   - Configure per-application routing rules

4. **Build the UI** in `ProxyPulse.UI`
   - Create setup wizard for first-time configuration
   - Implement dashboard for proxy status monitoring
   - Add application selection and routing configuration

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

![Build Status](https://img.shields.io/github/workflow/status/proxypulse/proxypulse/CI)
![Tests](https://img.shields.io/github/workflow/status/proxypulse/proxypulse/Tests?label=tests)
![Coverage](https://img.shields.io/codecov/c/github/proxypulse/proxypulse)

ProxyPulse is a Windows-only, single-EXE .NET 7 application that provides per-app SOCKS5 routing via real-time proxy APIs.

## Architecture

```ascii
                 ┌──────────────────┐
                 │   ProxyPulse.UI  │
                 │  (WPF + ReactUI) │
                 └─────────┬────────┘
                          │
         ┌───────────────┼───────────────┐
         │               │               │
┌────────▼─────┐ ┌──────▼───────┐ ┌────▼────────┐
│ ProxyPulse.  │ │  ProxyPulse. │ │ ProxyPulse. │
│   Fetchers   │ │   SocksCore  │ │   Routing   │
└──────┬───────┘ └──────┬───────┘ └─────┬───────┘
       │                │               │
       └────────────────┴───────────────┘
                       │
              ┌────────▼────────┐
              │  ProxyPulse.   │
              │    Common      │
              └───────────────-┘
```

## Prerequisites

None! ProxyPulse is distributed as a self-contained single EXE that includes everything needed to run.

## Quick Start

```powershell
# Clone the repository
git clone https://github.com/proxypulse/proxypulse.git
cd proxypulse

# Build the solution
dotnet build

# Run the application
dotnet run --project src/ProxyPulse.UI
```

## Project Structure

* **ProxyPulse.Common** - Shared models, settings & logging utilities
* **ProxyPulse.Fetchers** - `IProxyProvider` interface + proxy provider implementations
* **ProxyPulse.SocksCore** - `ISocksService` wrapper around a C# SOCKS5 engine
* **ProxyPulse.Routing** - `IRoutingService` with WinDivert driver & Proxifier support
* **ProxyPulse.UI** - ReactiveUI-based WPF application
* **ProxyPulse.Tests** - Unit, integration & UI tests

## License

MIT License - see the [LICENSE](LICENSE) file for details.
