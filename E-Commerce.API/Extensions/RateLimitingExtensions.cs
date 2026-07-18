using ECommerce.API.Helpers;
using ECommerce.Application.Common;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading;
using System.Threading.RateLimiting;

namespace ECommerce.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services , IConfiguration configuration)
    {
        var authLimit = configuration.GetValue<int>("RateLimitingSettings:AuthLimit");
        var catalogLimit = configuration.GetValue<int>("RateLimitingSettings:CatalogLimit");
        var checkoutLimit = configuration.GetValue<int>("RateLimitingSettings:CheckoutLimit");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";

                var userIp = context.HttpContext.Connection.RemoteIpAddress?.ToString()
                             ?? context.HttpContext.Request.Headers.Host.ToString();

                var apiResponse = ApiResponse.Fail($"Too many requests. Your IP ({userIp}) has been temporarily blocked for 1 minute.");

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonResult = JsonSerializer.Serialize(apiResponse, serializerOptions);
                await context.HttpContext.Response.WriteAsync(jsonResult, cancellationToken);
            };

            // Fixed window limiter for auth-related endpoints to protect against brute-force attempts
            options.AddPolicy("auth-policy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Token bucket limiter for catalog endpoints to allow bursts while capping request rate
            options.AddPolicy("catalog-policy", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = catalogLimit,
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                        TokensPerPeriod = 5
                    }));

            // Concurrency limiter for checkout endpoints to prevent too many simultaneous requests
            options.AddPolicy("checkout-policy", httpContext =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: GetPartitionKey(httpContext),
                    factory: _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = checkoutLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        return $"ip:{context.Connection.RemoteIpAddress}";
    }
}