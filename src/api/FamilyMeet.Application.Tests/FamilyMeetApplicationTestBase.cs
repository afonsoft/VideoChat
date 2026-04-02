using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Testing;
using FamilyMeet.Application.Services;
using FamilyMeet.Application.Contracts.Services;
using Moq;

namespace FamilyMeet.Application.Tests;

public abstract class FamilyMeetApplicationTestBase : AbpIntegratedTest<FamilyMeetApplicationTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddTransient(typeof(IChatAppService), typeof(ChatAppService));
        services.AddTransient(typeof(IChatMessageAppService), typeof(ChatMessageAppService));
        services.AddTransient(typeof(IVideoCallAppService), typeof(VideoCallAppService));
    }

    protected new TService GetService<TService>()
    {
        return GetRequiredService<TService>();
    }

    protected Mock<T> CreateMock<T>() where T : class
    {
        return new Mock<T>();
    }
}
