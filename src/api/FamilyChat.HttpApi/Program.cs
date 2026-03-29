using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SimpleConnect.HttpApi;
using SimpleConnect.HttpApi.Hubs;
using SimpleConnect.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ReplaceConfiguration(builder.Configuration);
builder.Services.AddApplication<SimpleConnectHttpApiModule>();
builder.Host.UseAutofac();

var app = builder.Build();

app.InitializeApplication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseAbpSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleConnect API");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub
app.MapHub<CommunicationHub>("/hubs/communication");

// Initialize Database and Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SimpleConnectDbContext>();

    // Create database if it doesn't exist
    await context.Database.EnsureCreatedAsync();

    // Seed initial data
    await context.SeedDataAsync();
}

app.Run();
