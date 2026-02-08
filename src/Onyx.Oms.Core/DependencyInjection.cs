using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Onyx.Oms.Core.Behaviors;
using System.Reflection;

namespace Onyx.Oms.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, params Assembly[] assemblies)
    {
        var allAssemblies = new[] { Assembly.GetExecutingAssembly() }.Concat(assemblies).ToArray();

        services.AddValidatorsFromAssemblies(allAssemblies);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(allAssemblies);

            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
