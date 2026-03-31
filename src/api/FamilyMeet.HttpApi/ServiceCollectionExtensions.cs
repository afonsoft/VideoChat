using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FamilyMeet.Application.Services;
using FamilyMeet.Application.Contracts.Services;
using FamilyMeet.Application;
using FamilyMeet.Domain.Repositories;
using FamilyMeet.EntityFrameworkCore.Repositories;
using FamilyMeet.EntityFrameworkCore;
using FamilyMeet.HttpApi.Services;
using FamilyMeet.HttpApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;

namespace FamilyMeet.HttpApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFamilyMeet(this IServiceCollection services, string connectionString)
    {
        // Entity Framework
        services.AddDbContext<FamilyMeetDbContext>(options =>
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
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(FamilyMeetAutoMapperProfile).Assembly));

        // SignalR
        services.AddSignalR();
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }
}
