using CubArt.Infrastructure.Caching;
using CubArt.Infrastructure.Common;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using CubArt.Infrastructure.Repositories;
using CubArt.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CubArt.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis cache
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")));

            services.AddScoped<IRedisCacheService, RedisCacheService>();

            // Database
            services.AddDbContext<AppDbContext>(builder =>
            {
                builder.UseNpgsql(configuration.GetConnectionString("DBConnectionString"), 
                    options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Log
            services.AddDbContext<LogDbContext>(builder =>
            {
                builder.UseNpgsql(configuration.GetConnectionString("DBConnectionString"));
                builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }, ServiceLifetime.Scoped);

            // Repositories
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ISupplyRepository, SupplyRepository>();
            services.AddScoped<IProductionRepository, ProductionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISystemLogRepository, SystemLogRepository>();

            // Services
            services.AddScoped<IStockMovementService, StockMovementService>();
            services.AddScoped<IDapperService, DapperService>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
