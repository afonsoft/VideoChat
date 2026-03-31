using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FamilyMeet.Domain;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using FamilyMeet.Application;
using FamilyMeet.Application.Services;

namespace FamilyMeet.EntityFrameworkCore;

[DependsOn(
    typeof(FamilyMeetDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class FamilyMeetEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FamilyMeetDbContext>(options =>
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
            options.InstanceName = "FamilyMeet:";
        });

        // Register Redis cache service
        context.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
    }
}
