using Template.Api.Common.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfiguredSerilog();

var app = builder.Build();

await app.RunAsync();
