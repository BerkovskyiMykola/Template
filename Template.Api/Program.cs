using System.Text;
using Microsoft.AspNetCore.HttpLogging;
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

var loggingFields = builder.Configuration.GetSection("HttpLogging:LoggingFields").Get<HttpLoggingFields>();
var requestHeaders = builder.Configuration.GetSection("HttpLogging:RequestHeaders").Get<string[]>() ?? [];
var responseHeaders = builder.Configuration.GetSection("HttpLogging:ResponseHeaders").Get<string[]>() ?? [];
var textMediaTypes = builder.Configuration.GetSection("HttpLogging:TextMediaTypes").Get<string[]>() ?? [];
var requestBodyLogLimit = builder.Configuration.GetSection("HttpLogging:RequestBodyLogLimit").Get<int>();
var responseBodyLogLimit = builder.Configuration.GetSection("HttpLogging:ResponseBodyLogLimit").Get<int>();

builder.Services.AddHttpLogging(configuration =>
{
    configuration.LoggingFields = loggingFields;

    configuration.RequestHeaders.Clear();

    foreach (var header in requestHeaders)
    {
        configuration.RequestHeaders.Add(header);
    }

    configuration.ResponseHeaders.Clear();

    foreach (var header in responseHeaders)
    {
        configuration.ResponseHeaders.Add(header);
    }

    configuration.MediaTypeOptions.Clear();

    foreach (var mediaType in textMediaTypes)
    {
        configuration.MediaTypeOptions.AddText(mediaType, Encoding.UTF8);
    }

    configuration.RequestBodyLogLimit = requestBodyLogLimit;
    configuration.ResponseBodyLogLimit = responseBodyLogLimit;
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();
