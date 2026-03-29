using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FamilyChat.Application.Services;
using FamilyChat.Application;
using FamilyChat.Domain.Repositories;
using FamilyChat.EntityFrameworkCore.Repositories;
using FamilyChat.EntityFrameworkCore;
using FamilyChat.HttpApi.Services;
using FamilyChat.HttpApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FamilyChat.HttpApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilyChat(this IServiceCollection services, string connectionString)
    {
        // Entity Framework
        services.AddDbContext<FamilyChatDbContext>(options =>
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
        services.AddAutoMapper(typeof(FamilyChatAutoMapperProfile));

        // SignalR
        services.AddSignalR();
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }
}
