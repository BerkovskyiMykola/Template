using Serilog;
using Template.Api.Common.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration.ReadFrom.Configuration(services.GetRequiredService<IConfiguration>());
    configuration.ReadFrom.Services(services);
});

builder.Services.AddTransient<CorrelationIdMiddleware>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

await app.RunAsync();
