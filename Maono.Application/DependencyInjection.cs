using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Maono.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline order: Logging → Validation → Transaction (commands only) → Handler
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.ValidationBehavior<,>));
            // TransactionBehavior is constrained to ICommand<> — it won't fire for IQuery<>
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
