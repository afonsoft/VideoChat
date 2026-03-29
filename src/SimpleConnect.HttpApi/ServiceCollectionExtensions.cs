using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleConnect.Application.Services;
using SimpleConnect.Application;
using SimpleConnect.Domain.Repositories;
using SimpleConnect.EntityFrameworkCore.Repositories;
using SimpleConnect.EntityFrameworkCore;
using SimpleConnect.HttpApi.Services;
using SimpleConnect.HttpApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SimpleConnect.HttpApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleConnect(this IServiceCollection services, string connectionString)
    {
        // Entity Framework
        services.AddDbContext<SimpleConnectDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Repositories
        services.AddScoped<IChatGroupRepository, ChatGroupRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IChatAppService, ChatAppService>();
        services.AddScoped<IChatMessageAppService, ChatMessageAppService>();
        services.AddScoped<IVideoCallAppService, VideoCallAppService>();

        // AutoMapper
        services.AddAutoMapper(typeof(SimpleConnectAutoMapperProfile));

        // SignalR
        services.AddSignalR();
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }
}
