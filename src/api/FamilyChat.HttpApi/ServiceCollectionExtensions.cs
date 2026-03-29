using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FamiyChat.Application.Services;
using FamiyChat.Application;
using FamiyChat.Domain.Repositories;
using FamiyChat.EntityFrameworkCore.Repositories;
using FamiyChat.EntityFrameworkCore;
using FamiyChat.HttpApi.Services;
using FamiyChat.HttpApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FamiyChat.HttpApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamiyChat(this IServiceCollection services, string connectionString)
    {
        // Entity Framework
        services.AddDbContext<FamiyChatDbContext>(options =>
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
        services.AddAutoMapper(typeof(FamiyChatAutoMapperProfile));

        // SignalR
        services.AddSignalR();
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }
}
