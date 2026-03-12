using BridgeApi.Application.Abstractions.Repositories.AppUser;
using BridgeApi.Application.Abstractions.Repositories.Connection;
using BridgeApi.Application.Abstractions.Repositories.Follow;
using BridgeApi.Application.Abstractions.Repositories.Intent;
using BridgeApi.Application.Abstractions.Repositories.Message;
using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostComment;
using BridgeApi.Application.Abstractions.Repositories.PostLike;
using BridgeApi.Application.Abstractions.Repositories.StoredFile;
using BridgeApi.Application.Abstractions.Repositories.UserIntent;
using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Persistence.Services.Auth;
using BridgeApi.Persistence.Contexts;
using AppUserEntity = BridgeApi.Domain.Entities.AppUser;
using BridgeApi.Persistence.Repositories.AppUser;
using BridgeApi.Persistence.Repositories.Connection;
using BridgeApi.Persistence.Repositories.Follow;
using BridgeApi.Persistence.Repositories.Intent;
using BridgeApi.Persistence.Repositories.Message;
using BridgeApi.Persistence.Repositories.Post;
using BridgeApi.Persistence.Repositories.PostComment;
using BridgeApi.Persistence.Repositories.PostLike;
using BridgeApi.Persistence.Repositories.StoredFile;
using BridgeApi.Persistence.Repositories.UserIntent;
using BridgeApi.Persistence.Repositories.UserProfile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeApi.Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddIdentity<AppUserEntity, BridgeApi.Domain.Entities.AppRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IAppUserReadRepository, AppUserReadRepository>();
        services.AddScoped<IAppUserWriteRepository, AppUserWriteRepository>();
        services.AddScoped<IUserProfileReadRepository, UserProfileReadRepository>();
        services.AddScoped<IUserProfileWriteRepository, UserProfileWriteRepository>();
        services.AddScoped<IIntentReadRepository, IntentReadRepository>();
        services.AddScoped<IIntentWriteRepository, IntentWriteRepository>();
        services.AddScoped<IConnectionReadRepository, ConnectionReadRepository>();
        services.AddScoped<IConnectionWriteRepository, ConnectionWriteRepository>();
        services.AddScoped<IMessageReadRepository, MessageReadRepository>();
        services.AddScoped<IMessageWriteRepository, MessageWriteRepository>();
        services.AddScoped<IPostReadRepository, PostReadRepository>();
        services.AddScoped<IPostWriteRepository, PostWriteRepository>();
        services.AddScoped<IPostCommentReadRepository, PostCommentReadRepository>();
        services.AddScoped<IPostCommentWriteRepository, PostCommentWriteRepository>();

        services.AddScoped<IPostLikeReadRepository, PostLikeReadRepository>();
        services.AddScoped<IPostLikeWriteRepository, PostLikeWriteRepository>();
        services.AddScoped<IFollowReadRepository, FollowReadRepository>();
        services.AddScoped<IFollowWriteRepository, FollowWriteRepository>();
        services.AddScoped<IUserIntentReadRepository, UserIntentReadRepository>();
        services.AddScoped<IUserIntentWriteRepository, UserIntentWriteRepository>();
        services.AddScoped<IStoredFileReadRepository, StoredFileReadRepository>();
        services.AddScoped<IStoredFileWriteRepository, StoredFileWriteRepository>();

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
