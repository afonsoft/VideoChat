using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using FamilyMeet.Domain;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using FamilyMeet.Application;
using FamilyMeet.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyMeet.EntityFrameworkCore;

[DependsOn(
    typeof(FamilyMeetDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule)
)]
public class FamilyMeetEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.AddAbpDbContext<FamilyMeetDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfiguration =>
            {
                abpDbContextConfiguration.DbContextOptions
                    .UseNpgsql(configuration.GetConnectionString("Default"), npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(FamilyMeetDbContext).Assembly.FullName);
                    });
            });
        });

        // Configure Redis caching
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