using Volo.Abp.Modularity;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Swashbuckle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FamiyChat.Domain;

namespace FamiyChat.HttpApi;

[DependsOn(
    typeof(FamiyChatDomainModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(AbpSwashbuckleModule)
)]
public class FamiyChatHttpApiModule : AbpModule
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
                        configuration["Jwt:Key"] ?? "FamiyChatSecretKey123456789"))
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
    }

    private void ConfigureSwaggerServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            "FamiyChat",
            opt =>
            {
                opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FamiyChat API", Version = "v1" });
                opt.DocInclusionPredicate((docName, description) => true);
                opt.CustomSchemaIds(type => type.FullName);
            },
            "FamiyChat",
            new string[] { "FamiyChat" }
        );
    }
}
