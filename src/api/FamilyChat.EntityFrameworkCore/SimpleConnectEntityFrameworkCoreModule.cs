using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleConnect.Domain;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using SimpleConnect.Application.Services;

namespace SimpleConnect.EntityFrameworkCore;

[DependsOn(
    typeof(SimpleConnectDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SimpleConnectEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SimpleConnectDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        // Configure Redis caching
        var configuration = context.Services.GetConfiguration();
        var redisHost = configuration["Redis:Host"] ?? "192.168.68.113";
        var redisPort = configuration["Redis:Port"] ?? "6379";
        var redisPassword = configuration["Redis:Password"];

        context.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{redisHost}:{redisPort}";
            if (!string.IsNullOrEmpty(redisPassword))
            {
                options.Configuration += $",password={redisPassword}";
            }
            options.InstanceName = "SimpleConnect:";
        });

        // Register Redis cache service
        context.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
    }
}
