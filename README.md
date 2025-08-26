# Template

`Template` serves as a template that can be used as an initial foundation for new projects. It provides best practices and a structured approach to service implementation.  

However, these best practices are not mandatory. If they do not suit the specific needs of a new project, they can be adjusted or replaced accordingly. The goal is to offer a useful starting point while allowing flexibility for project-specific requirements.

## NullGuard

This project uses **[NullGuard](https://github.com/Fody/NullGuard)** to automatically enforce null-safety at runtime. It injects null checks into method arguments and return values to catch potential `null` reference issues early.

## Observability

Observability in this project is implemented using structured logging, distributed tracing, and telemetry data to provide deep insights into system behavior and performance.

### Logging

- **[Serilog](https://serilog.net/)** is used as the primary logging framework.
- Logs are written in a structured format, making them suitable for filtering, searching, and analysis.
- The logging pipeline is enhanced with custom enrichers and supports contextual logging per request.

#### Custom Enrichers

The following enrichers add useful context to each log entry:

- **`TraceIdentifierEnricher`** – Adds the HTTP request's `TraceIdentifier`, helping trace logs tied to a specific request.
- **`UserIdentifierEnricher`** – Extracts and includes the authenticated user's identifier from the HTTP context, if available.
- **`CorrelationIdentifierEnricher`** – Adds a `X-Correlation-Id` from the request headers or generates one to enable distributed tracing across services.

These enrichers allow developers and operators to correlate logs across components and track issues down to specific users and requests.

#### High-Performance Logging with `LoggerMessageAttribute`

To minimize allocations and improve performance in high-frequency logging paths, this project uses [`LoggerMessageAttribute`](https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator):

- Log templates are defined statically and compiled into efficient logging methods.
- This avoids runtime string formatting and boxing of arguments.
- Ideal for performance-sensitive applications and microservices.

```csharp
[LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "User {UserId} logged in.")]
static partial void LogUserLogin(ILogger logger, string userId);
```

#### HTTP Request/Response Logging

This project supports configurable HTTP request and response logging using ASP.NET Core's built-in `AddHttpLogging` middleware.

- Logging of HTTP method, path, headers, and status codes can be enabled.
- Logging of request and response bodies is supported with configurable size limits.
- Useful for debugging, auditing, and traffic analysis.

> This functionality is optional and can be enabled or fine-tuned via configuration depending on the environment and sensitivity of data.

### Distributed Tracing and Metrics

To enhance observability beyond logs, this project uses **[OpenTelemetry](https://opentelemetry.io/)** to capture distributed traces and application metrics.

- **`Tracing`** is enabled for incoming HTTP requests, outgoing HTTP clients, and other operations.
- **`Metrics`** such as request duration, exception count, and system-level statistics (CPU, memory) are captured and exported.
- **`OTLP (OpenTelemetry Protocol)`** is supported for exporting telemetry to systems like Jaeger, Zipkin, Grafana, or Azure Monitor.

### Development Support with .NET Aspire

For local development and testing, this project optionally supports **[.NET Aspire](https://github.com/dotnet/aspire)** — an opinionated, cloud-ready stack for building observable and composable distributed applications.

- Aspire provides a developer dashboard for real-time inspection of logs, traces, and metrics.
- It simplifies the local setup of OpenTelemetry and related tooling.
- Ideal for testing observability scenarios without deploying to production infrastructure.

> Aspire is optional and can be used to accelerate development workflows, especially when working with multiple services.