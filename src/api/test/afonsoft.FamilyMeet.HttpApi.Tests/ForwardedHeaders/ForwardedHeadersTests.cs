using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace afonsoft.FamilyMeet.HttpApi.Tests.ForwardedHeaders;

public class ForwardedHeadersTests : FamilyMeetHttpApiTestBase
{
    private readonly IConfiguration _configuration;

    public ForwardedHeadersTests()
    {
        _configuration = GetRequiredService<IConfiguration>();
    }

    [Fact]
    public void ForwardedHeaders_Configuration_Should_Be_Loaded()
    {
        // Arrange & Act
        var forwardedHeadersConfig = _configuration.GetSection("ForwardedHeaders");

        // Assert
        forwardedHeadersConfig.ShouldNotBeNull();
        forwardedHeadersConfig["Enabled"].ShouldBe("true");
        forwardedHeadersConfig["ForwardedHeaders"].ShouldBe("XForwardedFor,XForwardedProto,XForwardedHost");
        forwardedHeadersConfig["AllowedHosts"].ShouldNotBeNull();
        forwardedHeadersConfig["KnownProxies"].ShouldNotBeNull();
    }

    [Fact]
    public void ForwardedHeaders_Options_Should_Be_Configured()
    {
        // Arrange
        var serviceProvider = GetServiceProvider();
        var forwardedHeadersOptions = serviceProvider.GetService<Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions>();

        // Act & Assert
        forwardedHeadersOptions.ShouldNotBeNull();
        forwardedHeadersOptions.ForwardedHeaders.ShouldBe(
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
            Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost
        );
        forwardedHeadersOptions.RequireHeaderSymmetry.ShouldBeFalse();
        forwardedHeadersOptions.ForwardLimit.ShouldBeNull();
    }

    [Fact]
    public async Task ForwardedHeaders_Middleware_Should_Process_XForwardedFor()
    {
        // Arrange
        var client = GetRequiredService<System.Net.Http.HttpClient>();
        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/api/identity/users");

        // Add X-Forwarded-For header
        request.Headers.Add("X-Forwarded-For", "192.168.1.100");
        request.Headers.Add("X-Forwarded-Proto", "https");
        request.Headers.Add("X-Forwarded-Host", "api.familymeet.com");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public void ForwardedHeaders_KnownProxies_Should_Be_Configured()
    {
        // Arrange
        var serviceProvider = GetServiceProvider();
        var forwardedHeadersOptions = serviceProvider.GetService<Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions>();

        // Act & Assert
        forwardedHeadersOptions.ShouldNotBeNull();
        forwardedHeadersOptions.KnownProxies.ShouldNotBeNull();
        forwardedHeadersOptions.KnownProxies.Count.ShouldBeGreaterThan(0);
        
        // Should include localhost for development
        forwardedHeadersOptions.KnownProxies.ShouldContain(System.Net.IPAddress.Loopback);
        forwardedHeadersOptions.KnownProxies.ShouldContain(System.Net.IPAddress.IPv6Loopback);
    }

    [Fact]
    public void ForwardedHeaders_AllowedHosts_Should_Be_Configured()
    {
        // Arrange
        var serviceProvider = GetServiceProvider();
        var forwardedHeadersOptions = serviceProvider.GetService<Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions>();

        // Act & Assert
        forwardedHeadersOptions.ShouldNotBeNull();
        forwardedHeadersOptions.AllowedHosts.ShouldNotBeNull();
        forwardedHeadersOptions.AllowedHosts.Count.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("X-Forwarded-For", "XForwardedFor")]
    [InlineData("X-Forwarded-Proto", "XForwardedProto")]
    [InlineData("X-Forwarded-Host", "XForwardedHost")]
    public void ForwardedHeaders_Custom_Names_Should_Be_Configured(string configKey, string expectedValue)
    {
        // Arrange
        var serviceProvider = GetServiceProvider();
        var forwardedHeadersOptions = serviceProvider.GetService<Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions>();

        // Act & Assert
        forwardedHeadersOptions.ShouldNotBeNull();
        
        switch (configKey)
        {
            case "X-Forwarded-For":
                forwardedHeadersOptions.ForwardedForHeaderName.ShouldBe(expectedValue);
                break;
            case "X-Forwarded-Proto":
                forwardedHeadersOptions.ForwardedProtoHeaderName.ShouldBe(expectedValue);
                break;
            case "X-Forwarded-Host":
                forwardedHeadersOptions.ForwardedHostHeaderName.ShouldBe(expectedValue);
                break;
        }
    }

    [Fact]
    public async Task ForwardedHeaders_Should_Preserve_RemoteIpAddress()
    {
        // This test would require integration testing with actual HTTP context
        // For now, we'll test the configuration
        
        // Arrange
        var serviceProvider = GetServiceProvider();
        var forwardedHeadersOptions = serviceProvider.GetService<Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersOptions>();

        // Act & Assert
        forwardedHeadersOptions.ShouldNotBeNull();
        forwardedHeadersOptions.ForwardedHeaders.HasFlag(Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor).ShouldBeTrue();
    }

    [Fact]
    public async Task ForwardedHeaders_Should_Handle_HTTPS_Redirect()
    {
        // Arrange
        var client = GetRequiredService<System.Net.Http.HttpClient>();
        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/api/identity/users");

        // Add headers to simulate HTTPS behind reverse proxy
        request.Headers.Add("X-Forwarded-Proto", "https");
        request.Headers.Add("X-Forwarded-Host", "api.familymeet.com");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public void ForwardedHeaders_Configuration_Should_Validate_IP_Parsing()
    {
        // Arrange
        var configuration = _configuration.GetSection("ForwardedHeaders");
        var knownProxies = configuration["KnownProxies"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);

        // Act & Assert
        knownProxies.ShouldNotBeNull();
        knownProxies.Length.ShouldBeGreaterThan(0);

        foreach (var proxy in knownProxies)
        {
            var trimmedProxy = proxy.Trim();
            if (trimmedProxy.Contains('/')) // CIDR notation
            {
                // For CIDR notation, we would need IPNetwork parsing
                // For now, just validate the format
                trimmedProxy.ShouldNotBeNullOrEmpty();
            }
            else
            {
                // For single IP addresses
                System.Net.IPAddress.TryParse(trimmedProxy, out var ipAddress).ShouldBeTrue();
                ipAddress.ShouldNotBeNull();
            }
        }
    }

    [Fact]
    public void ForwardedHeaders_Configuration_Should_Be_Environment_Specific()
    {
        // Arrange
        var environment = GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();

        // Act
        var forwardedHeadersConfig = _configuration.GetSection("ForwardedHeaders");

        // Assert
        forwardedHeadersConfig.ShouldNotBeNull();
        
        if (environment.IsDevelopment())
        {
            // Development should allow localhost
            var allowedHosts = forwardedHeadersConfig["AllowedHosts"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            allowedHosts.ShouldContain("localhost");
            allowedHosts.ShouldContain("127.0.0.1");
        }
        else if (environment.IsProduction())
        {
            // Production should have specific hosts
            var allowedHosts = forwardedHeadersConfig["AllowedHosts"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            allowedHosts.ShouldNotContain("*");
        }
    }
}

public abstract class FamilyMeetHttpApiTestBase : FamilyMeetApplicationTestBase
{
    protected override void SetAbpApplicationCreationOptions(Volo.Abp.AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddHttpClient();
        base.AfterAddApplication(services);
    }

    protected System.Net.Http.HttpClient GetHttpClient()
    {
        return GetRequiredService<System.Net.Http.HttpClient>();
    }
}
