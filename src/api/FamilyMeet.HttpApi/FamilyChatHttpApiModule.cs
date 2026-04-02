using Volo.Abp.Modularity;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Swashbuckle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FamilyMeet.Domain;
using FamilyMeet.EntityFrameworkCore;
using FamilyMeet.HttpApi.Services;
using Volo.Abp.AspNetCore.SignalR;
using Prometheus;
using Serilog;

namespace FamilyMeet.HttpApi;

[DependsOn(
    typeof(FamilyMeetDomainModule),
    typeof(FamilyMeetEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSignalRModule)
)]
public class FamilyMeetHttpApiModule : AbpModule
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
                        configuration["Jwt:Key"] ?? "FamilyMeetSecretKey123456789"))
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

        // Configure Prometheus
        ConfigurePrometheus(context);

        // Register ConnectionManager
        context.Services.AddSingleton<IConnectionManager, ConnectionManager>();
    }

    private void ConfigureSwaggerServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // Read the AuthServer authority and a swagger client id from configuration
        // Provide safe defaults to avoid passing null into ABP helpers
        var authServerAuthority = configuration["AuthServer:Authority"] ?? "https://localhost:5001";
        var swaggerClientId = configuration["AuthServer:SwaggerClientId"] ?? "FamilyMeet";

        context.Services.AddAbpSwaggerGenWithOAuth(
            authServerAuthority,
            new Dictionary<string, string>
            {
                {"FamilyMeet", "FamilyMeet API"}
            },
            null,
            swaggerClientId
        );
    }

    private void ConfigurePrometheus(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var prometheusEnabled = configuration.GetValue<bool>("Prometheus:Enabled", true);

        if (prometheusEnabled)
        {
            // Configurar Serilog para Prometheus
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console();

            // Adicionar sink do Prometheus se estiver disponível
            var prometheusEndpoint = configuration.GetValue<string>("Prometheus:Endpoint");
            if (!string.IsNullOrEmpty(prometheusEndpoint))
            {
                loggerConfig.WriteTo.Prometheus(prometheusEndpoint);
            }

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
