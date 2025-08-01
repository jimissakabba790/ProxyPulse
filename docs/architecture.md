# ProxyPulse Architecture

## System Context

```ascii
                          ┌─────────────┐
                          │   Windows   │
                          │   System    │
                          └─────────────┘
                                ▲
                                │
                    ┌──────────┴──────────┐
                    │    ProxyPulse UI    │
                    └──────────┬──────────┘
                          ┌────┴────┐
           ┌──────────────┤  Core   ├──────────────┐
           │             └────┬────┘              │
           ▼                  ▼                    ▼
┌──────────────────┐  ┌────────────┐    ┌─────────────────┐
│ Proxy Fetchers   │  │ SOCKS Core │    │ Routing Service │
└──────────────────┘  └────────────┘    └────────┬────────┘
           │                                      │
           ▼                                      ▼
┌──────────────────┐                    ┌─────────────────┐
│   Provider APIs  │                    │ WinDivert Driver│
└──────────────────┘                    └─────────────────┘
```

## Component Overview

### ProxyPulse UI
- WPF-based user interface using ReactiveUI/MVVM
- Handles user interactions and configuration
- Displays proxy status, routing rules, and system tray integration
- Manages automatic updates and telemetry

### Proxy Fetchers
- Pluggable provider architecture (`IProxyProvider`)
- Validates and ranks proxy servers
- Handles rate limiting and caching
- Supports multiple provider APIs

### SOCKS Core
- SOCKS5 protocol implementation
- Connection pool management
- Authentication handling
- Health monitoring and failover

### Routing Service
- Process-level traffic routing
- Rule-based routing decisions
- Network interface management
- Traffic monitoring and statistics

### WinDivert Driver
- Kernel-mode packet interception
- TCP/UDP traffic redirection
- NAT and connection tracking
- High-performance packet processing

## Key Features

- **Modularity**: Each component is independently testable and replaceable
- **Resilience**: Automatic failover and retry mechanisms
- **Performance**: Optimized for minimal latency overhead
- **Security**: Secure by default with optional authentication
- **Observability**: Comprehensive logging and telemetry

## Data Flow

1. UI configures routing rules and proxy settings
2. Fetchers maintain fresh proxy server pool
3. SocksCore establishes proxy connections
4. RoutingService intercepts matching traffic
5. WinDivert redirects packets through SOCKS proxy

## Configuration

All components are configured via `appsettings.json` with environment overrides.
Services use dependency injection for loose coupling and testability.
