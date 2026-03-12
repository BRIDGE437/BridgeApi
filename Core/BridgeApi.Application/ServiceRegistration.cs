using BridgeApi.Application.Behaviours;
using BridgeApi.Application.Validators.User;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeApi.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        return services;
    }
}
