using Volo.Abp.Modularity;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Swashbuckle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FamilyChat.Domain;
using FamilyChat.EntityFrameworkCore;
using FamilyChat.HttpApi.Services;
using Volo.Abp.AspNetCore.SignalR;

namespace FamilyChat.HttpApi;

[DependsOn(
    typeof(FamilyChatDomainModule),
    typeof(FamilyChatEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSignalRModule)
)]
public class FamilyChatHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var services = context.Services;

        // Configure JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["Jwt:Key"] ?? "FamilyChatSecretKey123456789"))
                };
            });

        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Configure Swagger
        ConfigureSwaggerServices(context);

        // Register ConnectionManager
        context.Services.AddSingleton<IConnectionManager, ConnectionManager>();
    }

    private void ConfigureSwaggerServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // Read the AuthServer authority and a swagger client id from configuration
        // Provide safe defaults to avoid passing null into ABP helpers
        var authServerAuthority = configuration["AuthServer:Authority"] ?? "https://localhost:5001";
        var swaggerClientId = configuration["AuthServer:SwaggerClientId"] ?? "FamilyChat";

        context.Services.AddAbpSwaggerGenWithOAuth(
            authServerAuthority,
            new Dictionary<string, string>
            {
                {"FamilyChat", "FamilyChat API"}
            },
            null,
            swaggerClientId
        );
    }
}