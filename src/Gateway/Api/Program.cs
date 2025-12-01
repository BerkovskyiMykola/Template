/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();

app.UseHttpsRedirection();

await app.RunAsync().ConfigureAwait(false);
