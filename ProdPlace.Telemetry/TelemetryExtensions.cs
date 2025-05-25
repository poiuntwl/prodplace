using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ProdPlace.Telemetry;

public static class TelemetryExtensions
{
    private class TelemetryConfigurationAnnouncerProcessor : BaseProcessor<Activity>
    {
        // This processor doesn't need to do anything with activities.
        // Its sole purpose is to be a vehicle for the factory that does the logging.
        // Or, the logging can be done in its constructor if preferred.
    }

    public static IServiceCollection AddSharedOpenTelemetry(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion,
        IConfiguration configuration,
        Type callerType)
    {
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(serviceName)
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.client_ip",
                                httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString());
                            activity.SetTag("http.user_agent", httpRequest.Headers.UserAgent.ToString());
                            var correlationId = httpRequest.Headers["X-Correlation-Id"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(correlationId))
                            {
                                activity.SetTag("http.correlation_id", correlationId);
                            }
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response_status_code", httpResponse.StatusCode);
                            activity.SetTag("http.response_length", httpResponse.ContentLength ?? 0);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("http.uri", httpRequestMessage.RequestUri?.ToString());
                            activity.SetTag("http.method", httpRequestMessage.Method.ToString());
                            var requestId = httpRequestMessage.Headers.TryGetValues("Request-Id", out var values) ? values.FirstOrDefault() : null;
                            if (!string.IsNullOrEmpty(requestId))
                            {
                                activity.SetTag("http.request_id", requestId);
                            }
                        };
                        options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                        {
                            activity.SetTag("http.response_status_code", (int)httpResponseMessage.StatusCode);
                        };
                    })
                    .AddGrpcClientInstrumentation(options =>
                    {
                        options.SuppressDownstreamInstrumentation = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("grpc.method", httpRequestMessage.RequestUri?.AbsolutePath);
                            activity.SetTag("grpc.service", httpRequestMessage.RequestUri?.Host);
                        };
                        options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                        {
                            var grpcStatus = httpResponseMessage.Headers.TryGetValues("grpc-status", out var values) ? values.FirstOrDefault() : null;
                            if (!string.IsNullOrEmpty(grpcStatus))
                            {
                                activity.SetTag("grpc.status_code", grpcStatus);
                            }
                        };
                    });

                var jaegerOtlpEndpoint = configuration["Otel:Exporter:Otlp:Endpoint"];
                var useOtlp = !string.IsNullOrWhiteSpace(jaegerOtlpEndpoint);

                if (useOtlp)
                {
                    tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(jaegerOtlpEndpoint);
                        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
                else
                {
                    tracerProviderBuilder.AddConsoleExporter(options =>
                        options.Targets = ConsoleExporterOutputTargets.Console);

                    // Use AddProcessor with a factory to get IServiceProvider for logging
                    tracerProviderBuilder.AddProcessor(sp =>
                    {
                        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger(callerType); // Use the provided type
                        logger.LogInformation(
                            "Jaeger OTLP endpoint not configured for service {ServiceName}. Using ConsoleExporter for tracing.",
                            serviceName);

                        // Return a no-op processor instance
                        return new TelemetryConfigurationAnnouncerProcessor();
                    });
                }
            });

        return services;
    }
}