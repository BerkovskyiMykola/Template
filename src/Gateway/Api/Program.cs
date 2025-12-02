/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Serialization / Formatting
builder.Services.ConfigureHttpJsonOptions(config => { });
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    #pragma warning disable IDE0058
    app.UseDeveloperExceptionPage();
    #pragma warning restore IDE0058
}

app.UseHttpsRedirection();

await app.RunAsync().ConfigureAwait(false);
