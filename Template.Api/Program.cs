using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration.ReadFrom.Configuration(services.GetRequiredService<IConfiguration>());
    configuration.ReadFrom.Services(services);
});

var app = builder.Build();

await app.RunAsync();
