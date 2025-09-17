# Template

`Template` is a collection of solutions for various common problems that may arise during development. It provides ready-to-use approaches and examples that can serve as a foundation for building new projects.  

At the same time, using these solutions is not mandatory: they can be modified, combined, or replaced to fit the specific requirements of each project.

## Project Configuration

To ensure consistency, maintainability, and code quality across the solution, this project includes several configuration files.  
They standardize development practices, centralize shared settings, and help catch issues early in the development cycle.

### Code Style, Formatting and Analysis

- **`.editorconfig`** defines both **code style and formatting** rules as well as **static code analysis** settings.  
  This ensures a single source of truth for consistent coding standards and analyzer enforcement across the solution.

### Build and Dependency Management

- **`Directory.Build.props`** centralizes MSBuild properties such as target frameworks, compiler settings, and analysis options.
  This eliminates duplication and makes it easier to apply changes across multiple projects.

- **`Directory.Packages.props`** manages NuGet package versions in one place.  
  It prevents version conflicts, enforces consistency, and simplifies dependency upgrades across the solution.

## Internal Logic Validation 

Ensuring correctness of internal methods and class invariants can be challenging, especially during development when assumptions may change frequently.  
`ThrowHelper.Debug` provides a lightweight solution for validating internal logic without impacting production performance.

### Problem

- Internal methods often rely on assumptions about parameters, state, or calculations.
- Mistakes or invalid inputs can lead to subtle bugs that are hard to trace.
- Validation is often only needed during development and debugging, not in production.

### Solution

`ThrowHelper.Debug` offers a centralized set of methods to validate conditions and throw exceptions in debug builds:

### Benefits

- Rapid detection of logical errors during development.
- Guarantees that invalid states are caught early, preventing cascading bugs.
- No performance penalty in production builds.

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