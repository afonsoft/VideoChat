using Volo.Abp.Modularity;
using Volo.Abp.AutoMapper;
using FamiyChat.Application.Contracts;

namespace FamiyChat.Application;

[DependsOn(
    typeof(FamiyChatApplicationContractsModule),
    typeof(AbpAutoMapperModule)
)]
public class FamiyChatApplicationModule : AbpModule
{
}
