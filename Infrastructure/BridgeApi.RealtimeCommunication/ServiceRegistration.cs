using BridgeApi.Application.Abstractions.Services;
using BridgeApi.RealtimeCommunication.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BridgeApi.RealtimeCommunication;

public static class ServiceRegistration
{
    public static IServiceCollection AddRealtimeCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis:ConnectionString not found.");

        services.AddSignalR()
            .AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("BridgeSignalR");
            });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddSingleton<IPresenceService, RedisPresenceService>();
        services.AddSingleton<IRealtimeNotificationService, RealtimeNotificationService>();

        return services;
    }
}
