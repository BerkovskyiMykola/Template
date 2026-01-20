/*
 * Api
 * Copyright (c) 2025-2025 Mykola Berkovskyi
 */

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Diagnostics.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Net.Http.Headers;

namespace Api.Common.Logging;

/// <summary>  
/// Extension methods for configuring logging in the application. 
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>  
    /// Adds an HttpLogging to the <see cref="IServiceCollection"/> if the
    /// "HttpLogging" configuration section exists in the <see cref="IConfiguration"/>.  
    /// </summary>  
    /// <remarks>
    /// Argument validation is performed only in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="services">The services to which the HttpLogging will be added.</param>  
    /// <param name="configuration">The configuration used to check whether the HttpLogging is configured.</param>  
    /// <returns>The original services.</returns> 
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddHttpLoggingIfConfigured(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        #if DEBUG
        Guard.IsNotNull(services);
        Guard.IsNotNull(configuration);
        #endif

        MyHttpLoggingOptions? options = configuration.GetSection("HttpLogging").Get<MyHttpLoggingOptions>();

        if (options is null)
        {
            return services;
        }

        services.AddHttpLogging(options.ConfigureHttpLoggingOptions);

        services.AddHttpLoggingRedaction(options => { });

        return services;
    }

    private sealed record MyHttpLoggingOptions
    {
        public HttpLoggingFields? LoggingFields 
        { 
            get;

            #pragma warning disable S1144
            init
            #pragma warning restore S1144
            {
                if (value is not null)
                {
                    GuardExt.IsDefinedFlagsEnumCombination(value.Value);
                }

                field = value;
            } 
        }

        public string[]? RequestHeaders 
        { 
            get;

            #pragma warning disable S1144
            init
            #pragma warning restore S1144
            {
                if (value is not null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(value[i]))
                        {
                            throw new ArgumentException("Request header cannot be null or whitespace.", nameof(value));
                        }
                    }
                }

                field = value;
            }
        }

        public string[]? ResponseHeaders 
        { 
            get;

            #pragma warning disable S1144
            init
            #pragma warning restore S1144
            {
                if (value is not null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(value[i]))
                        {
                            throw new ArgumentException("Response header cannot be null or whitespace.", nameof(value));
                        }
                    }
                }

                field = value;
            }
        }

        public string[]? TextMediaTypes 
        { 
            get;

            #pragma warning disable S1144
            init
            #pragma warning restore S1144
            {
                if (value is not null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(value[i]))
                        {
                            throw new ArgumentException("Text media type cannot be null or whitespace.", nameof(value));
                        }

                        if (!MediaTypeHeaderValue.TryParse(value[i], out _))
                        {
                            throw new ArgumentException($"Text media type '{value[i]}' is not a valid media type.", nameof(value));
                        }
                    }
                }

                field = value;
            }
        }

        public int? RequestBodyLogLimit 
        { 
            get;
            
            #pragma warning disable S1144
            init
            #pragma warning restore S1144
            {
                if (value is not null)
                {
                    Guard.IsGreaterThan(value.Value, 0);
                }

                field = value;
            }
        }

        public int? ResponseBodyLogLimit 
        { 
            get;
            
            #pragma warning disable S1144
            init
            #pragma warning restore S1144
            {
                if (value is not null)
                {
                    Guard.IsGreaterThan(value.Value, 0);
                }

                field = value;
            }
        }

        public void ConfigureHttpLoggingOptions(HttpLoggingOptions options)
        {
            #if DEBUG
            Guard.IsNotNull(options);
            #endif

            if (LoggingFields is not null)
            {
                options.LoggingFields = LoggingFields.Value;
            }

            if (RequestHeaders is not null)
            {
                options.RequestHeaders.Clear();

                foreach (string header in RequestHeaders)
                {
                    _ = options.RequestHeaders.Add(header);
                }
            }

            if (ResponseHeaders is not null)
            {
                options.ResponseHeaders.Clear();

                foreach (string header in ResponseHeaders)
                {
                    _ = options.ResponseHeaders.Add(header);
                }
            }

            if (TextMediaTypes is not null)
            {
                options.MediaTypeOptions.Clear();

                foreach (string textMediaType in TextMediaTypes)
                {
                    options.MediaTypeOptions.AddText(textMediaType);
                }
            }

            if (RequestBodyLogLimit is not null)
            {
                options.RequestBodyLogLimit = RequestBodyLogLimit.Value;
            }

            if (ResponseBodyLogLimit is not null)
            {
                options.ResponseBodyLogLimit = ResponseBodyLogLimit.Value;
            }
        }
    }
}
