using Volo.Abp.Modularity;
using Volo.Abp.EntityFrameworkCore;
using SimpleConnect.Domain.Shared;

namespace SimpleConnect.Domain;

[DependsOn(
    typeof(SimpleConnectDomainSharedModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SimpleConnectDomainModule : AbpModule
{
}
