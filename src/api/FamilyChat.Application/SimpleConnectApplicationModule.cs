using Volo.Abp.Modularity;
using Volo.Abp.AutoMapper;
using SimpleConnect.Application.Contracts;

namespace SimpleConnect.Application;

[DependsOn(
    typeof(SimpleConnectApplicationContractsModule),
    typeof(AbpAutoMapperModule)
)]
public class SimpleConnectApplicationModule : AbpModule
{
}
