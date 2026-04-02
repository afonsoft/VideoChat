using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FamilyMeet.HttpApi;
using FamilyMeet.HttpApi.Hubs;
using FamilyMeet.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.SignalR;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ReplaceConfiguration(builder.Configuration);
builder.Services.AddApplication<FamilyMeetHttpApiModule>();
builder.Host.UseAutofac();

var app = builder.Build();

app.InitializeApplication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseAbpSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FamilyMeet API");
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
    var context = scope.ServiceProvider.GetRequiredService<FamilyMeetDbContext>();

    // Create database if it doesn't exist
    await context.Database.EnsureCreatedAsync();

    // Seed initial data
    await context.SeedDataAsync();
}

app.Run();
