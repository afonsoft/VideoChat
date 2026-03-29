using Volo.Abp.Modularity;
using SimpleConnect.Domain.Shared;

namespace SimpleConnect.Application.Contracts;

[DependsOn(
    typeof(SimpleConnectDomainSharedModule)
)]
public class SimpleConnectApplicationContractsModule : AbpModule
{
}
