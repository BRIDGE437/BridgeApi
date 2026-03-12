using Amazon;
using Amazon.S3;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos;
using BridgeApi.Infrastructure.Configuration;
using BridgeApi.Infrastructure.Services.Caching;
using BridgeApi.Infrastructure.Services.Storage;
using BridgeApi.Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeApi.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleAuth"));
        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName = configuration["Redis:InstanceName"];
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Storage:Provider"]?.ToLowerInvariant();

        if (provider == "s3")
        {
            services.Configure<S3StorageSettings>(configuration.GetSection("Storage:S3"));

            var s3Settings = configuration.GetSection("Storage:S3").Get<S3StorageSettings>()!;
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                s3Settings.AccessKey,
                s3Settings.SecretKey,
                RegionEndpoint.GetBySystemName(s3Settings.Region)));

            services.AddScoped<IStorageService, S3StorageService>();
        }
        else
        {
            services.Configure<LocalStorageSettings>(configuration.GetSection("Storage:Local"));
            services.AddScoped<IStorageService, LocalStorageService>();
        }

        return services;
    }
}
