using AutoMapper;
using CubArt.Application.Common.BackgroundServices;
using CubArt.Application.Common.Behaviors;
using CubArt.Application.Common.Mapping;
using CubArt.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CubArt.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Behaviors
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Background services
            services.AddHostedService<StockBalanceRecalculationService>();

            // AutoMapper - явная регистрация
            services.AddSingleton<IMapper>(provider =>
            {
                //var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

                var configuration = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<MappingProfile>();
                });

                return configuration.CreateMapper();
            });

            // Services
            services.AddScoped<IAuthService, AuthService>();

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
