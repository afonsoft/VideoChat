using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FamiyChat.Domain;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using FamiyChat.Application.Services;

namespace FamiyChat.EntityFrameworkCore;

[DependsOn(
    typeof(FamiyChatDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class FamiyChatEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FamiyChatDbContext>(options =>
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
            options.InstanceName = "FamiyChat:";
        });

        // Register Redis cache service
        context.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
    }
}
