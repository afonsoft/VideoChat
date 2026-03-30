using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FamilyChat.Domain;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using FamilyChat.Application;
using FamilyChat.Application.Services;

namespace FamilyChat.EntityFrameworkCore;

[DependsOn(
    typeof(FamilyChatDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class FamilyChatEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FamilyChatDbContext>(options =>
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
            options.InstanceName = "FamilyChat:";
        });

        // Register Redis cache service
        context.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
    }
}
